using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Claims;
using AspNetCoreRateLimit;
using AspNetCoreRateLimit.Redis;
using DqtApi.Configuration;
using DqtApi.DataStore.Crm;
using DqtApi.DataStore.Sql;
using DqtApi.Filters;
using DqtApi.Json;
using DqtApi.Logging;
using DqtApi.ModelBinding;
using DqtApi.Security;
using DqtApi.Swagger;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Microsoft.PowerPlatform.Dataverse.Client;
using Npgsql;
using Prometheus;
using Sentry.AspNetCore;
using Sentry.Extensibility;
using Serilog;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DqtApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            TypeDescriptor.AddAttributes(typeof(DateOnly), new TypeConverterAttribute(typeof(DateOnlyTypeConverter)));

            var builder = WebApplication.CreateBuilder(args);

            var services = builder.Services;
            var env = builder.Environment;
            var configuration = builder.Configuration;

            builder.Host.UseSerilog((ctx, config) => config.ReadFrom.Configuration(ctx.Configuration));

            if (env.IsProduction())
            {
                builder.WebHost.UseSentry();
            }

            if (builder.Environment.IsProduction())
            {
                builder.Configuration
                    .AddJsonEnvironmentVariable("AppConfig")
                    .AddJsonEnvironmentVariable("VCAP_SERVICES", configurationKeyPrefix: "VCAP_SERVICES")
                    .AddJsonEnvironmentVariable("VCAP_APPLICATION", configurationKeyPrefix: "VCAP_APPLICATION");
            }

            services.AddAuthentication(ApiKeyAuthenticationHandler.AuthenticationScheme)
                .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationHandler.AuthenticationScheme, _ => { });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    "Bearer",
                    policy => policy
                        .AddAuthenticationSchemes(ApiKeyAuthenticationHandler.AuthenticationScheme)
                        .RequireClaim(ClaimTypes.Name));

                options.DefaultPolicy = options.GetPolicy("Bearer");
            });

            services
                .AddMvc(options =>
                {
                    options.AddHybridBodyModelBinderProvider();

                    options.Filters.Add(new AuthorizeFilter());
                    options.Filters.Add(new ProducesJsonOrProblemAttribute());
                    options.Filters.Add(new CrmServiceProtectionFaultExceptionFilter());
                    options.Filters.Add(new DefaultErrorExceptionFilter(statusCode: StatusCodes.Status400BadRequest));

                    options.Conventions.Add(new ApiVersionConvention());
                })
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.SuppressInferBindingSourcesForParameters = true;
                })
                .AddFluentValidation(fv =>
                {
                    fv.RegisterValidatorsFromAssemblyContaining(typeof(Program));
                    fv.DisableDataAnnotationsValidation = true;
                });

            services.AddTransient<IApiDescriptionProvider, HybridBodyApiDescriptionProvider>();

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.AddConverters();
                });

            services.Decorate<Microsoft.AspNetCore.Mvc.Infrastructure.ProblemDetailsFactory, CamelCaseErrorKeysProblemDetailsFactory>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo() { Title = "DQT API", Version = "v1" });
                c.SwaggerDoc("v2", new OpenApiInfo() { Title = "DQT API", Version = "v2" });

                c.DocInclusionPredicate((docName, api) => docName.Equals(api.GroupName, StringComparison.OrdinalIgnoreCase));
                c.EnableAnnotations();
                c.ExampleFilters();
                c.OperationFilter<ResponseContentTypeOperationFilter>();
                c.OperationFilter<RateLimitOperationFilter>();

                c.CustomSchemaIds(type =>
                {
                    // Generated CRM models for custom entities have a weird type name; fix that up here
                    // e.g. for the 'dfeta_inductionState' type use 'InductionState' in the API spec

                    if (type.Name.StartsWith("dfeta_"))
                    {
                        var prefixTrimmedTypeName = type.Name.Substring("dfeta_".Length);
                        return prefixTrimmedTypeName[0..1].ToUpper() + prefixTrimmedTypeName[1..];
                    }

                    return type.Name;
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Scheme = "Bearer",
                    Type = SecuritySchemeType.Http
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    [
                        new OpenApiSecurityScheme()
                        {
                            Reference = new OpenApiReference()
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        }
                    ] = new List<string>()
                });
            });

            services.Configure<SentryAspNetCoreOptions>(options =>
            {
                var paasEnvironmentName = configuration["PaasEnvironment"];
                if (!string.IsNullOrEmpty(paasEnvironmentName))
                {
                    options.Environment = paasEnvironmentName;
                }

                var gitSha = configuration["GitSha"];
                if (!string.IsNullOrEmpty(gitSha))
                {
                    options.Release = gitSha;
                }
            });

            var pgConnectionString = GetPostgresConnectionString();

            var healthCheckBuilder = services.AddHealthChecks()
                .AddNpgSql(pgConnectionString);

            services.AddMediatR(typeof(Program));
            services.AddSingleton<IApiClientRepository, ConfigurationApiClientRepository>();
            services.AddSingleton<ICurrentClientProvider, ClaimsPrincipalCurrentClientProvider>();
            services.AddSwaggerExamplesFromAssemblyOf<Program>();
            services.AddTransient<ISerializerDataContractResolver>(sp =>
            {
                var serializerOptions = sp.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions;
                return new Swagger.JsonSerializerDataContractResolver(serializerOptions);
            });
            services.AddSingleton<IClock, Clock>();
            services.AddMemoryCache();
            services.AddSingleton<ISentryEventProcessor, RemoveRedactedUrlParametersEventProcessor>();

            services.AddDbContext<DqtContext>(options =>
            {
                DqtContext.ConfigureOptions(options, pgConnectionString);
            });

            services.AddDatabaseDeveloperPageExceptionFilter();

            if (env.EnvironmentName != "Testing")
            {
                var crmServiceClient = GetCrmServiceClient();

                services.AddSingleton<IOrganizationServiceAsync>(crmServiceClient);
                services.AddSingleton<IDataverseAdapter, DataverseAdapter>();

                healthCheckBuilder.AddCheck("CRM", () => crmServiceClient.IsReady ? HealthCheckResult.Healthy() : HealthCheckResult.Degraded());
            }

            if (env.IsProduction())
            {
                ConfigureRateLimitServices();
                ConfigureRedisServices();
            }

            MetricLabels.ConfigureLabels(builder.Configuration);

            var app = builder.Build();

            app.UseRequestLogging();

            app.UseRouting();

            if (env.IsProduction())
            {
                app.UseSentryTracing();
            }

            app.UseHttpMetrics();

            app.UseHealthChecks("/status");

            app.UseAuthentication();
            app.UseAuthorization();

            if (env.IsProduction())
            {
                app.UseMiddleware<RateLimitMiddleware>();
            }

            app.Use((ctx, next) =>
            {
                ctx.Response.Headers.Add("X-Frame-Options", "deny");
                ctx.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                ctx.Response.Headers.Add("X-XSS-Protection", "0");

                return next();
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/health", async context =>
                {
                    await context.Response.WriteAsync("OK");
                });

                endpoints.MapMetrics();

                endpoints.MapControllers();
            });

            app.UseSwagger(options =>
            {
                options.PreSerializeFilters.Add((_, request) =>
                {
                    request.HttpContext.Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
                });
            });

            if (env.IsDevelopment())
            {
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("v1/swagger.json", "DQT API v1");
                    c.SwaggerEndpoint("v2/swagger.json", "DQT API v2");
                    c.EnablePersistAuthorization();
                });

                app.UseMigrationsEndPoint();

                using (var scope = app.Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<DqtContext>();
                    context.Database.EnsureCreated();
                }
            }

            app.Run();

            ServiceClient GetCrmServiceClient() =>
                new ServiceClient(
                    new Uri(configuration["CrmUrl"]),
                    configuration["CrmClientId"],
                    configuration["CrmClientSecret"],
                    useUniqueInstance: true);

            void ConfigureRateLimitServices()
            {
                services.Configure<ClientRateLimitOptions>(configuration.GetSection("ClientRateLimiting"));

                services.AddDistributedRateLimiting<AsyncKeyLockProcessingStrategy>();
                services.AddDistributedRateLimiting<RedisProcessingStrategy>();
                services.AddRedisRateLimiting();

                services.AddSingleton<IClientPolicyStore, DistributedCacheClientPolicyStore>();
                services.AddSingleton<IRateLimitCounterStore, DistributedCacheRateLimitCounterStore>();
                services.AddSingleton<IRateLimitConfiguration, Security.RateLimitConfiguration>();
            }

            void ConfigureRedisServices()
            {
                var connectionString = configuration.GetConnectionString("Redis") ?? GetConnectionStringForPaasService();

                services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(connectionString));
                services.AddStackExchangeRedisCache(options => options.Configuration = connectionString);

                healthCheckBuilder.AddRedis(connectionString);

                string GetConnectionStringForPaasService()
                {
                    var options = new ConfigurationOptions()
                    {
                        EndPoints =
                        {
                            {
                                configuration.GetValue<string>("VCAP_SERVICES:redis:0:credentials:host"),
                                configuration.GetValue<int>("VCAP_SERVICES:redis:0:credentials:port")
                            }
                        },
                        Password = configuration.GetValue<string>("VCAP_SERVICES:redis:0:credentials:password"),
                        Ssl = configuration.GetValue<bool>("VCAP_SERVICES:redis:0:credentials:tls_enabled")
                    };

                    return options.ToString();
                }
            }

            string GetPostgresConnectionString()
            {
                return configuration.GetConnectionString("DefaultConnection") ?? GetConnectionStringForPaasService();

                string GetConnectionStringForPaasService()
                {
                    var builder = new NpgsqlConnectionStringBuilder()
                    {
                        Host = configuration.GetValue<string>("VCAP_SERVICES:postgres:0:credentials:host"),
                        Database = configuration.GetValue<string>("VCAP_SERVICES:postgres:0:credentials:name"),
                        Username = configuration.GetValue<string>("VCAP_SERVICES:postgres:0:credentials:username"),
                        Password = configuration.GetValue<string>("VCAP_SERVICES:postgres:0:credentials:password"),
                        Port = configuration.GetValue<int>("VCAP_SERVICES:postgres:0:credentials:port"),
                        SslMode = SslMode.Require,
                        TrustServerCertificate = true
                    };

                    return builder.ConnectionString;
                }
            }
        }
    }
}
