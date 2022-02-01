using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using WrappedResolver = Swashbuckle.AspNetCore.SwaggerGen.JsonSerializerDataContractResolver;

namespace DqtApi.Swagger
{
    // Decorates Swashbuckle's built-in JsonSerializerDataContractResolver to add in support for additional types e.g. DateOnly
    // see https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/f6dc5f872d6d065324c21b37852a1bdf8858420e/src/Swashbuckle.AspNetCore.SwaggerGen/SchemaGenerator/JsonSerializerDataContractResolver.cs
    public class JsonSerializerDataContractResolver : ISerializerDataContractResolver
    {
        private readonly WrappedResolver _innerResolver;
        private readonly JsonSerializerOptions _serializerOptions;

        public JsonSerializerDataContractResolver(JsonSerializerOptions serializerOptions)
        {
            _innerResolver = new WrappedResolver(serializerOptions);
            _serializerOptions = serializerOptions;
        }

        public DataContract GetDataContractForType(Type type)
        {
            if (type == typeof(DateOnly))
            {
                return DataContract.ForPrimitive(
                    underlyingType: type,
                    dataType: DataType.String,
                    dataFormat: "date",
                    jsonConverter: value => JsonSerializer.Serialize(value, _serializerOptions));
            }

            var contract = _innerResolver.GetDataContractForType(type);

            // Omit any URL-bound properties from the contract
            if (type.GetProperties().Any(IsPropertyUrlBound))
            {
                return CreateObjectContractWithoutUrlBoundProperties();
            }

            return contract;

            static bool IsPropertyUrlBound(MemberInfo property) =>
                property.GetCustomAttribute<FromRouteAttribute>() != null || property.GetCustomAttribute<FromQueryAttribute>() != null;

            DataContract CreateObjectContractWithoutUrlBoundProperties() =>
                DataContract.ForObject(
                    contract.UnderlyingType,
                    contract.ObjectProperties.Where(p => !IsPropertyUrlBound(p.MemberInfo)),
                    contract.ObjectExtensionDataType,
                    contract.ObjectTypeNameProperty,
                    contract.ObjectTypeNameValue,
                    contract.JsonConverter);
        }
    }
}
