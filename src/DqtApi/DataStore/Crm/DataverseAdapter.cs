using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Threading.Tasks;
using DqtApi.DataStore.Crm.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.PowerPlatform.Dataverse.Client.Utils;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace DqtApi.DataStore.Crm
{
    public partial class DataverseAdapter : IDataverseAdapter
    {
        private readonly IOrganizationServiceAsync _service;
        private readonly IClock _clock;
        private readonly IMemoryCache _cache;

        public DataverseAdapter(
            IOrganizationServiceAsync organizationServiceAsync,
            IClock clock,
            IMemoryCache cache)
        {
            _service = organizationServiceAsync;
            _clock = clock;
            _cache = cache;
        }

        public Task<dfeta_country> GetCountry(string value) => GetCountry(value, requestBuilder: null);

        public async Task<dfeta_country> GetCountry(string value, RequestBuilder requestBuilder)
        {
            requestBuilder ??= RequestBuilder.CreateSingle(_service);

            var query = new QueryByAttribute(dfeta_country.EntityLogicalName)
            {
                ColumnSet = new() { AllColumns = true }
            };

            query.AddAttributeValue(dfeta_country.Fields.dfeta_Value, value);
            query.AddAttributeValue(dfeta_country.Fields.StateCode, (int)dfeta_countryState.Active);

            var request = new RetrieveMultipleRequest()
            {
                Query = query
            };

            var result = await requestBuilder.AddRequest<RetrieveMultipleResponse>(request).GetResponseAsync();

            return result.EntityCollection.Entities.Select(entity => entity.ToEntity<dfeta_country>()).FirstOrDefault();
        }

        public Task<dfeta_earlyyearsstatus> GetEarlyYearsStatus(string value) => GetEarlyYearsStatus(value, requestBuilder: null);

        public async Task<dfeta_earlyyearsstatus> GetEarlyYearsStatus(string value, RequestBuilder requestBuilder)
        {
            requestBuilder ??= RequestBuilder.CreateSingle(_service);

            var query = new QueryByAttribute(dfeta_earlyyearsstatus.EntityLogicalName)
            {
                ColumnSet = new() { AllColumns = true }
            };

            query.AddAttributeValue(dfeta_earlyyearsstatus.Fields.dfeta_Value, value);
            query.AddAttributeValue(dfeta_earlyyearsstatus.Fields.StateCode, (int)dfeta_earlyyearsStatusState.Active);

            var request = new RetrieveMultipleRequest()
            {
                Query = query
            };

            var result = await requestBuilder.AddRequest<RetrieveMultipleResponse>(request).GetResponseAsync();

            return result.EntityCollection.Entities.Select(entity => entity.ToEntity<dfeta_earlyyearsstatus>()).FirstOrDefault();
        }

        public Task<dfeta_hequalification> GetHeQualificationByCode(string value) => GetHeQualificationByCode(value, requestBuilder: null);

        public async Task<dfeta_hequalification> GetHeQualificationByCode(string value, RequestBuilder requestBuilder)
        {
            requestBuilder ??= RequestBuilder.CreateSingle(_service);

            var query = new QueryByAttribute(dfeta_hequalification.EntityLogicalName)
            {
                ColumnSet = new() { AllColumns = true }
            };

            query.AddAttributeValue(dfeta_hequalification.Fields.dfeta_Value, value);
            query.AddAttributeValue(dfeta_hequalification.Fields.StateCode, (int)dfeta_hequalificationState.Active);

            var request = new RetrieveMultipleRequest()
            {
                Query = query
            };

            var result = await requestBuilder.AddRequest<RetrieveMultipleResponse>(request).GetResponseAsync();

            return result.EntityCollection.Entities.Select(entity => entity.ToEntity<dfeta_hequalification>()).FirstOrDefault();
        }

        public Task<dfeta_hesubject> GetHeSubjectByCode(string value) => GetHeSubjectByCode(value, requestBuilder: null);

        public async Task<dfeta_hesubject> GetHeSubjectByCode(string value, RequestBuilder requestBuilder)
        {
            requestBuilder ??= RequestBuilder.CreateSingle(_service);

            var query = new QueryByAttribute(dfeta_hesubject.EntityLogicalName)
            {
                ColumnSet = new() { AllColumns = true }
            };

            query.AddAttributeValue(dfeta_hesubject.Fields.dfeta_Value, value);
            query.AddAttributeValue(dfeta_hesubject.Fields.StateCode, (int)dfeta_hesubjectState.Active);

            var request = new RetrieveMultipleRequest()
            {
                Query = query
            };

            var result = await requestBuilder.AddRequest<RetrieveMultipleResponse>(request).GetResponseAsync();

            return result.EntityCollection.Entities.Select(entity => entity.ToEntity<dfeta_hesubject>()).FirstOrDefault();
        }

        public Task<dfeta_initialteachertraining[]> GetInitialTeacherTrainingByTeacher(
            Guid teacherId,
            params string[] columnNames) =>
                GetInitialTeacherTrainingByTeacher(teacherId, columnNames, requestBuilder: null);

        public async Task<dfeta_initialteachertraining[]> GetInitialTeacherTrainingByTeacher(
            Guid teacherId,
            string[] columnNames,
            RequestBuilder requestBuilder)
        {
            requestBuilder ??= RequestBuilder.CreateSingle(_service);

            var query = new QueryByAttribute(dfeta_initialteachertraining.EntityLogicalName)
            {
                ColumnSet = new(columnNames)
            };

            query.AddAttributeValue(dfeta_initialteachertraining.Fields.dfeta_PersonId, teacherId);
            query.AddAttributeValue(dfeta_initialteachertraining.Fields.StateCode, (int)dfeta_initialteachertrainingState.Active);

            var request = new RetrieveMultipleRequest()
            {
                Query = query
            };

            var result = await requestBuilder.AddRequest<RetrieveMultipleResponse>(request).GetResponseAsync();

            return result.EntityCollection.Entities.Select(entity => entity.ToEntity<dfeta_initialteachertraining>()).ToArray();
        }

        public async Task<Account[]> GetIttProviders(bool activeOnly)
        {
            var filter = new FilterExpression(LogicalOperator.And);
            if (activeOnly)
                filter.AddCondition(Account.Fields.StateCode, ConditionOperator.Equal, (int)AccountState.Active);
            filter.AddCondition(Account.Fields.dfeta_TrainingProvider, ConditionOperator.Equal, true);
            filter.AddCondition(Account.Fields.dfeta_UKPRN, ConditionOperator.NotNull);

            var query = new QueryExpression(Account.EntityLogicalName)
            {
                ColumnSet = new(
                    Account.Fields.Name,
                    Account.Fields.dfeta_UKPRN),
                Criteria = filter,
                Orders =
                {
                    new OrderExpression(Account.Fields.Name, OrderType.Ascending)
                }
            };

            var result = await _service.RetrieveMultipleAsync(query);

            return result.Entities.Select(entity => entity.ToEntity<Account>()).ToArray();
        }

        public Task<dfeta_ittqualification> GetIttQualificationByCode(string code) => GetIttQualificationByCode(code, requestBuilder: null);

        public async Task<dfeta_ittqualification> GetIttQualificationByCode(string code, RequestBuilder requestBuilder)
        {
            requestBuilder ??= RequestBuilder.CreateSingle(_service);

            var query = new QueryByAttribute(dfeta_ittqualification.EntityLogicalName)
            {
                ColumnSet = new() { AllColumns = true }
            };

            query.AddAttributeValue(dfeta_ittqualification.Fields.dfeta_Value, code);
            query.AddAttributeValue(dfeta_ittqualification.Fields.StateCode, (int)dfeta_ittqualificationState.Active);

            var request = new RetrieveMultipleRequest()
            {
                Query = query
            };

            var result = await requestBuilder.AddRequest<RetrieveMultipleResponse>(request).GetResponseAsync();

            return result.EntityCollection.Entities.Select(entity => entity.ToEntity<dfeta_ittqualification>()).FirstOrDefault();
        }

        public Task<dfeta_ittsubject> GetIttSubjectByCode(string code) => GetIttSubjectByCode(code, requestBuilder: null);

        public async Task<dfeta_ittsubject> GetIttSubjectByCode(string code, RequestBuilder requestBuilder)
        {
            requestBuilder ??= RequestBuilder.CreateSingle(_service);

            var query = new QueryByAttribute(dfeta_ittsubject.EntityLogicalName)
            {
                ColumnSet = new() { AllColumns = true }
            };

            query.AddAttributeValue(dfeta_ittsubject.Fields.dfeta_Value, code);
            query.AddAttributeValue(dfeta_ittsubject.Fields.StateCode, (int)dfeta_ittsubjectState.Active);

            var request = new RetrieveMultipleRequest()
            {
                Query = query
            };

            var result = await requestBuilder.AddRequest<RetrieveMultipleResponse>(request).GetResponseAsync();

            return result.EntityCollection.Entities.Select(entity => entity.ToEntity<dfeta_ittsubject>()).FirstOrDefault();
        }

        public async Task<Contact[]> FindTeachers(FindTeachersByTrnBirthDateAndNinoQuery request)
        {
            var filter = new FilterExpression(LogicalOperator.And);

            if (string.IsNullOrEmpty(request.NationalInsuranceNumber))
            {
                filter.AddCondition(Contact.Fields.dfeta_TRN, ConditionOperator.Equal, request.Trn);
            }
            else
            {
                var childFilter = new FilterExpression(LogicalOperator.Or);

                childFilter.AddCondition(Contact.Fields.dfeta_TRN, ConditionOperator.Equal, request.Trn);
                childFilter.AddCondition(Contact.Fields.dfeta_NINumber, ConditionOperator.Equal, request.NationalInsuranceNumber);

                filter.AddFilter(childFilter);
            }

            filter.AddCondition(Contact.Fields.BirthDate, ConditionOperator.Equal, request.BirthDate);

            var query = new QueryExpression(Contact.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(
                    Contact.Fields.FullName,
                    Contact.Fields.StateCode,
                    Contact.Fields.dfeta_TRN,
                    Contact.Fields.dfeta_NINumber,
                    Contact.Fields.BirthDate,
                    Contact.Fields.dfeta_ActiveSanctions
                ),
                Criteria = filter
            };

            AddInductionLink(query);
            AddInitialTeacherTrainingLink(query);
            AddQualifiedTeacherStatusLink(query);

            var result = await _service.RetrieveMultipleAsync(query);

            return result.Entities.Select(entity => entity.ToEntity<Contact>()).ToArray();

            static void AddInitialTeacherTrainingLink(QueryExpression query)
            {
                var initialTeacherTrainingLink = query.AddLink(
                    dfeta_initialteachertraining.EntityLogicalName,
                    Contact.PrimaryIdAttribute,
                    dfeta_initialteachertraining.Fields.dfeta_PersonId,
                    JoinOperator.LeftOuter);

                initialTeacherTrainingLink.Columns = new ColumnSet(
                    dfeta_initialteachertraining.PrimaryIdAttribute,
                    dfeta_initialteachertraining.Fields.StateCode,
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeType,
                    dfeta_initialteachertraining.Fields.dfeta_ITTQualificationId,
                    dfeta_initialteachertraining.Fields.dfeta_Result,
                    dfeta_initialteachertraining.Fields.dfeta_Subject1Id,
                    dfeta_initialteachertraining.Fields.dfeta_Subject2Id,
                    dfeta_initialteachertraining.Fields.dfeta_Subject3Id,
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeStartDate,
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeEndDate
                );

                initialTeacherTrainingLink.EntityAlias = nameof(dfeta_initialteachertraining);

                AddSubjectLinks(initialTeacherTrainingLink);
            }

            static void AddSubjectLinks(LinkEntity initialTeacherTrainingLink)
            {
                var aliasPrefix = nameof(dfeta_initialteachertraining) + "." + nameof(dfeta_ittsubject);

                AddSubjectLink(initialTeacherTrainingLink, dfeta_initialteachertraining.Fields.dfeta_Subject1Id, aliasPrefix + 1);
                AddSubjectLink(initialTeacherTrainingLink, dfeta_initialteachertraining.Fields.dfeta_Subject2Id, aliasPrefix + 2);
                AddSubjectLink(initialTeacherTrainingLink, dfeta_initialteachertraining.Fields.dfeta_Subject3Id, aliasPrefix + 3);
            }

            static void AddSubjectLink(LinkEntity initialTeacherTrainingLink, string subjectIdField, string alias)
            {
                var subjectLink = initialTeacherTrainingLink.AddLink(
                    dfeta_ittsubject.EntityLogicalName,
                    subjectIdField,
                    dfeta_ittsubject.PrimaryIdAttribute,
                    JoinOperator.LeftOuter);

                subjectLink.Columns = new ColumnSet(dfeta_ittsubject.Fields.dfeta_Value);

                subjectLink.EntityAlias = alias;
            }

            static void AddInductionLink(QueryExpression query)
            {
                var inductionLink = query.AddLink(
                    dfeta_induction.EntityLogicalName,
                    Contact.PrimaryIdAttribute,
                    dfeta_induction.Fields.dfeta_PersonId,
                    JoinOperator.LeftOuter);

                inductionLink.Columns = new ColumnSet(
                    dfeta_induction.PrimaryIdAttribute,
                    dfeta_induction.Fields.dfeta_InductionStatus,
                    dfeta_induction.Fields.StateCode,
                    dfeta_induction.Fields.dfeta_StartDate,
                    dfeta_induction.Fields.dfeta_CompletionDate
                );

                inductionLink.EntityAlias = nameof(dfeta_induction);
            }

            static void AddQualifiedTeacherStatusLink(QueryExpression query)
            {
                var qualifiedTeacherStatusLink = query.AddLink(
                    dfeta_qtsregistration.EntityLogicalName,
                    Contact.PrimaryIdAttribute,
                    dfeta_qtsregistration.Fields.dfeta_PersonId,
                    JoinOperator.LeftOuter);

                qualifiedTeacherStatusLink.Columns = new ColumnSet(
                    dfeta_qtsregistration.PrimaryIdAttribute,
                    dfeta_qtsregistration.Fields.dfeta_name,
                    dfeta_qtsregistration.Fields.StateCode,
                    dfeta_qtsregistration.Fields.dfeta_QTSDate
                );

                qualifiedTeacherStatusLink.EntityAlias = nameof(dfeta_qtsregistration);

                var filter = new FilterExpression();
                filter.AddCondition(dfeta_initialteachertraining.Fields.StateCode, ConditionOperator.Equal, (int)dfeta_initialteachertrainingState.Active);
                qualifiedTeacherStatusLink.LinkCriteria = filter;
            }
        }

        public Task<Account[]> GetIttProviderOrganizationsByName(string name, bool activeOnly, params string[] columnNames) =>
            GetIttProviderOrganizationsByName(name, activeOnly, columnNames, requestBuilder: null);

        public async Task<Account[]> GetIttProviderOrganizationsByName(string name, bool activeOnly, string[] columnNames, RequestBuilder requestBuilder)
        {
            requestBuilder ??= RequestBuilder.CreateSingle(_service);

            var query = new QueryByAttribute(Account.EntityLogicalName)
            {
                ColumnSet = new(columnNames)
            };

            query.AddAttributeValue(Account.Fields.dfeta_TrainingProvider, true);
            query.AddAttributeValue(Account.Fields.Name, name);

            if (activeOnly)
            {
                query.AddAttributeValue(Account.Fields.StateCode, (int)AccountState.Active);
            }

            var request = new RetrieveMultipleRequest()
            {
                Query = query
            };

            var result = await requestBuilder.AddRequest<RetrieveMultipleResponse>(request).GetResponseAsync();

            return result.EntityCollection.Entities.Select(entity => entity.ToEntity<Account>()).ToArray();
        }

        public Task<Account[]> GetIttProviderOrganizationsByUkprn(string ukprn, bool activeOnly, params string[] columnNames) =>
            GetIttProviderOrganizationsByUkprn(ukprn, activeOnly, columnNames, requestBuilder: null);

        public async Task<Account[]> GetIttProviderOrganizationsByUkprn(string ukprn, bool activeOnly, string[] columnNames, RequestBuilder requestBuilder)
        {
            requestBuilder ??= RequestBuilder.CreateSingle(_service);

            var query = new QueryByAttribute(Account.EntityLogicalName)
            {
                ColumnSet = new(columnNames)
            };

            query.AddAttributeValue(Account.Fields.dfeta_TrainingProvider, true);
            query.AddAttributeValue(Account.Fields.dfeta_UKPRN, ukprn);

            if (activeOnly)
                query.AddAttributeValue(Account.Fields.StateCode, (int)AccountState.Active);

            var request = new RetrieveMultipleRequest()
            {
                Query = query
            };

            var result = await requestBuilder.AddRequest<RetrieveMultipleResponse>(request).GetResponseAsync();

            return result.EntityCollection.Entities.Select(entity => entity.ToEntity<Account>()).ToArray();
        }

        public Task<Account[]> GetOrganizationsByUkprn(string ukprn, params string[] columnNames) =>
            GetOrganizationsByUkprn(ukprn, columnNames, requestBuilder: null);

        public async Task<Account[]> GetOrganizationsByUkprn(string ukprn, string[] columnNames, RequestBuilder requestBuilder)
        {
            requestBuilder ??= RequestBuilder.CreateSingle(_service);

            var query = new QueryByAttribute(Account.EntityLogicalName)
            {
                ColumnSet = new(columnNames)
            };

            query.AddAttributeValue(Account.Fields.dfeta_UKPRN, ukprn);
            query.AddAttributeValue(Account.Fields.StateCode, (int)AccountState.Active);

            var request = new RetrieveMultipleRequest()
            {
                Query = query
            };

            var result = await requestBuilder.AddRequest<RetrieveMultipleResponse>(request).GetResponseAsync();

            return result.EntityCollection.Entities.Select(entity => entity.ToEntity<Account>()).ToArray();
        }

        public Task<dfeta_qtsregistration[]> GetQtsRegistrationsByTeacher(
            Guid teacherId,
            params string[] columnNames) =>
                GetQtsRegistrationsByTeacher(teacherId, columnNames, requestBuilder: null);

        public async Task<dfeta_qtsregistration[]> GetQtsRegistrationsByTeacher(
            Guid teacherId,
            string[] columnNames,
            RequestBuilder requestBuilder)
        {
            requestBuilder ??= RequestBuilder.CreateSingle(_service);

            var query = new QueryByAttribute(dfeta_qtsregistration.EntityLogicalName)
            {
                ColumnSet = new(columnNames)
            };

            query.AddAttributeValue(dfeta_qtsregistration.Fields.dfeta_PersonId, teacherId);
            query.AddAttributeValue(dfeta_qtsregistration.Fields.StateCode, (int)dfeta_qtsregistrationState.Active);

            var request = new RetrieveMultipleRequest()
            {
                Query = query
            };

            var result = await requestBuilder.AddRequest<RetrieveMultipleResponse>(request).GetResponseAsync();

            return result.EntityCollection.Entities.Select(entity => entity.ToEntity<dfeta_qtsregistration>()).ToArray();
        }

        public async Task<dfeta_qualification[]> GetQualificationsForTeacher(Guid teacherId, params string[] columnNames)
        {
            var filter = new FilterExpression();
            filter.AddCondition(dfeta_qualification.Fields.dfeta_PersonId, ConditionOperator.Equal, teacherId);
            filter.AddCondition(dfeta_qtsregistration.Fields.StateCode, ConditionOperator.Equal, (int)dfeta_qualificationState.Active);

            var query = new QueryExpression(dfeta_qualification.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(columnNames),
                Criteria = filter
            };

            AddHeQualificationLink(query);
            AddSubjectLinks(query);

            var result = await _service.RetrieveMultipleAsync(query);

            return result.Entities.Select(entity => entity.ToEntity<dfeta_qualification>()).ToArray();

            static void AddHeQualificationLink(QueryExpression query)
            {
                var heSubjectLink = query.AddLink(
                    dfeta_hequalification.EntityLogicalName,
                    dfeta_qualification.Fields.dfeta_HE_HEQualificationId,
                    dfeta_hequalification.Fields.Id,
                    JoinOperator.LeftOuter);

                heSubjectLink.Columns = new ColumnSet(dfeta_hequalification.PrimaryIdAttribute, dfeta_hequalification.Fields.dfeta_name);

                heSubjectLink.EntityAlias = dfeta_hequalification.EntityLogicalName;
            }

            static void AddSubjectLinks(QueryExpression query)
            {
                var aliasPrefix = dfeta_hesubject.EntityLogicalName;

                AddSubjectLink(query, dfeta_qualification.Fields.dfeta_HE_HESubject1Id, aliasPrefix + 1);
                AddSubjectLink(query, dfeta_qualification.Fields.dfeta_HE_HESubject2Id, aliasPrefix + 2);
                AddSubjectLink(query, dfeta_qualification.Fields.dfeta_HE_HESubject3Id, aliasPrefix + 3);
            }

            static void AddSubjectLink(QueryExpression query, string subjectIdField, string alias)
            {
                var subjectLink = query.AddLink(
                    dfeta_hesubject.EntityLogicalName,
                    subjectIdField,
                    dfeta_hesubject.PrimaryIdAttribute,
                    JoinOperator.LeftOuter);

                subjectLink.Columns = new ColumnSet(
                    dfeta_hesubject.PrimaryIdAttribute,
                    dfeta_hesubject.Fields.dfeta_name,
                    dfeta_hesubject.Fields.dfeta_Value);

                subjectLink.EntityAlias = alias;
            }
        }

        public async Task<Contact> GetTeacher(Guid teacherId, bool resolveMerges = true, params string[] columnNames)
        {
            var columnSet = new ColumnSet(
                columnNames
                    .Append(Contact.Fields.Merged)
                    .Append(Contact.Fields.MasterId)
                    .Distinct()
                    .ToArray());

            Contact teacher;

            try
            {
                teacher = (await _service.RetrieveAsync(Contact.EntityLogicalName, teacherId, columnSet)).ToEntity<Contact>();
            }
            catch (FaultException<OrganizationServiceFault> fault) when (fault.Message.Contains($"{teacherId} Does Not Exist"))
            {
                return null;
            }

            while (resolveMerges && teacher.Merged == true)
            {
                var masterReference = teacher.MasterId;
                return await GetTeacher(masterReference.Id, resolveMerges, columnNames);
            }

            return teacher;
        }

        public async Task<Contact[]> GetTeachersByTrn(string trn, bool activeOnly = true, params string[] columnNames)
        {
            var filter = new FilterExpression(LogicalOperator.And);
            filter.AddCondition(Contact.Fields.dfeta_TRN, ConditionOperator.Equal, trn);
            if (activeOnly)
            {
                filter.AddCondition(Contact.Fields.StateCode, ConditionOperator.Equal, (int)ContactState.Active);
            }

            var query = new QueryExpression(Contact.EntityLogicalName)
            {
                ColumnSet = new(columnNames),
                Criteria = filter
            };

            var result = await _service.RetrieveMultipleAsync(query);

            return result.Entities.Select(e => e.ToEntity<Contact>()).ToArray();
        }

        public async Task<Contact[]> GetTeachersByTrnAndDoB(string trn, DateOnly birthDate, bool activeOnly = true, params string[] columnNames)
        {
            var filter = new FilterExpression(LogicalOperator.And);
            filter.AddCondition(Contact.Fields.dfeta_TRN, ConditionOperator.Equal, trn);
            filter.AddCondition(Contact.Fields.BirthDate, ConditionOperator.Equal, birthDate.ToDateTime());
            if (activeOnly)
            {
                filter.AddCondition(Contact.Fields.StateCode, ConditionOperator.Equal, (int)ContactState.Active);
            }

            var query = new QueryExpression(Contact.EntityLogicalName)
            {
                ColumnSet = new(columnNames),
                Criteria = filter
            };

            var result = await _service.RetrieveMultipleAsync(query);

            return result.Entities.Select(e => e.ToEntity<Contact>()).ToArray();
        }

        public Task<CrmTask[]> GetCrmTasksForTeacher(Guid teacherId, params string[] columnNames) =>
            GetCrmTasksForTeacher(teacherId, columnNames, requestBuilder: null);

        public async Task<CrmTask[]> GetCrmTasksForTeacher(Guid teacherId, string[] columnNames, RequestBuilder requestBuilder)
        {
            requestBuilder ??= RequestBuilder.CreateSingle(_service);

            var query = new QueryByAttribute(CrmTask.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(columnNames)
            };
            query.AddAttributeValue(CrmTask.Fields.RegardingObjectId, teacherId);

            var request = new RetrieveMultipleRequest()
            {
                Query = query
            };

            var result = await requestBuilder.AddRequest<RetrieveMultipleResponse>(request).GetResponseAsync();

            return result.EntityCollection.Entities.Select(entity => entity.ToEntity<CrmTask>()).ToArray();
        }

        public Task<dfeta_teacherstatus> GetTeacherStatus(
            string value,
            bool qtsDateRequired) =>
                GetTeacherStatus(value, qtsDateRequired, requestBuilder: null);

        public async Task<dfeta_teacherstatus> GetTeacherStatus(
            string value,
            bool qtsDateRequired,
            RequestBuilder requestBuilder)
        {
            // TECH DEBT Some junk reference data in the build environment means we have teacher statuses duplicated.
            // In some cases the duplicate records vary by 'dfeta_qtsdaterequired' - we need to ensure we get the correct
            // one as a workflow will prevent us allocating a qtsregistration for a status where dfeta_qtsdaterequired is true
            // without a QTS Date.

            requestBuilder ??= RequestBuilder.CreateSingle(_service);

            var query = new QueryByAttribute(dfeta_teacherstatus.EntityLogicalName)
            {
                ColumnSet = new() { AllColumns = true }
            };

            query.AddAttributeValue(dfeta_teacherstatus.Fields.dfeta_Value, value);
            query.AddAttributeValue(dfeta_teacherstatus.Fields.StateCode, (int)dfeta_teacherStatusState.Active);
            query.AddAttributeValue(dfeta_teacherstatus.Fields.dfeta_QTSDateRequired, qtsDateRequired);

            var request = new RetrieveMultipleRequest()
            {
                Query = query
            };

            var result = await requestBuilder.AddRequest<RetrieveMultipleResponse>(request).GetResponseAsync();

            return result.EntityCollection.Entities.Select(entity => entity.ToEntity<dfeta_teacherstatus>()).FirstOrDefault();
        }

        public async Task<bool> UnlockTeacherRecord(Guid teacherId)
        {
            var update = new Entity(Contact.EntityLogicalName, teacherId);
            update[Contact.Fields.dfeta_loginfailedcounter] = 0;

            try
            {
                await _service.UpdateAsync(update);
                return true;
            }
            catch (DataverseOperationException ex)
                when (ex.InnerException is Microsoft.Rest.HttpOperationException httpException && httpException.Response.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        public Task<Account[]> GetOrganizationsByName(string name, bool activeOnly, params string[] columnNames) =>
            GetOrganizationsByName(name, activeOnly, columnNames, requestBuilder: null);

        public async Task<Account[]> GetOrganizationsByName(string name, bool activeOnly, string[] columnNames, RequestBuilder requestBuilder)
        {
            requestBuilder ??= RequestBuilder.CreateSingle(_service);

            var query = new QueryByAttribute(Account.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(columnNames)
            };

            query.AddAttributeValue(Account.Fields.Name, name);

            if (activeOnly)
                query.AddAttributeValue(Account.Fields.StateCode, (int)AccountState.Active);

            var request = new RetrieveMultipleRequest()
            {
                Query = query
            };

            var result = await requestBuilder.AddRequest<RetrieveMultipleResponse>(request).GetResponseAsync();

            return result.EntityCollection.Entities.Select(entity => entity.ToEntity<Account>()).ToArray();
        }

        public async Task<Contact[]> FindTeachers(FindTeachersQuery findTeachersQuery)
        {
            var conditions = new List<object>();  //  Either ConditionExpresssion or FilterExpression

            void AddEqualCondition(string fieldName, object value)
            {
                if (value is null || (value is string str && string.IsNullOrEmpty(str)))
                {
                    return;
                }

                conditions.Add(new ConditionExpression(fieldName, ConditionOperator.Equal, value));
            }

            AddEqualCondition(Contact.Fields.BirthDate, findTeachersQuery.DateOfBirth?.ToDateTime());
            AddEqualCondition(Contact.Fields.dfeta_NINumber, findTeachersQuery.NationalInsuranceNumber);

            {
                // Find all the permutations of names to match on
                var firstNames = new[] { findTeachersQuery.FirstName, findTeachersQuery.PreviousFirstName };
                var lastNames = new[] { findTeachersQuery.LastName, findTeachersQuery.PreviousLastName };

                var firstNamesFilter = new FilterExpression(LogicalOperator.Or);
                foreach (var firstName in firstNames)
                {
                    if (!string.IsNullOrEmpty(firstName))
                    {
                        firstNamesFilter.AddCondition(Contact.Fields.FirstName, ConditionOperator.Equal, firstName);
                    }
                }

                var lastNamesFilter = new FilterExpression(LogicalOperator.Or);
                foreach (var lastName in lastNames)
                {
                    if (!string.IsNullOrEmpty(lastName))
                    {
                        lastNamesFilter.AddCondition(Contact.Fields.LastName, ConditionOperator.Equal, lastName);
                    }
                }

                var nameFilter = new FilterExpression(LogicalOperator.And);

                if (firstNamesFilter.Conditions.Count > 0)
                {
                    nameFilter.AddFilter(firstNamesFilter);
                }

                if (lastNamesFilter.Conditions.Count > 0)
                {
                    nameFilter.AddFilter(lastNamesFilter);
                }

                if (nameFilter.Filters.Count > 0)
                {
                    conditions.Add(nameFilter);
                }
            }

            LinkEntity ittProviderLink = null;
            var ittProviderOrganizationIdsArray = findTeachersQuery.IttProviderOrganizationIds?.ToArray() ?? Array.Empty<Guid>();

            if (ittProviderOrganizationIdsArray.Length > 0)
            {
                ittProviderLink = new LinkEntity(
                    Contact.EntityLogicalName,
                    dfeta_initialteachertraining.EntityLogicalName,
                    Contact.PrimaryIdAttribute,
                    dfeta_initialteachertraining.Fields.dfeta_PersonId,
                    JoinOperator.LeftOuter)
                {
                    Columns = new ColumnSet(dfeta_initialteachertraining.Fields.dfeta_EstablishmentId)
                };

                conditions.Add(
                    new ConditionExpression(
                        dfeta_initialteachertraining.EntityLogicalName,
                        dfeta_initialteachertraining.Fields.dfeta_EstablishmentId,
                        ConditionOperator.In,
                        ittProviderOrganizationIdsArray.Cast<object>().ToArray()));  // https://community.dynamics.com/crm/b/crmbusiness/posts/crm-2011-odd-error-with-query-expression-and-conditionoperator-in
            }

            // If we still don't have at least 3 identifiers to match on then we're done
            var identifierCount = conditions.Count;
            if (identifierCount < 3)
            {
                return Array.Empty<Contact>();
            }

            // Get all permutations of 3 matching conditions
            var filter = new FilterExpression(LogicalOperator.Or);
            foreach (var permutationConditions in conditions.GetCombinations(length: 3))
            {
                var innerFilter = new FilterExpression(LogicalOperator.And);

                foreach (var condition in permutationConditions)
                {
                    if (condition is FilterExpression filterCondition)
                    {
                        innerFilter.AddFilter(filterCondition);
                    }
                    else
                    {
                        Debug.Assert(condition is ConditionExpression);
                        innerFilter.AddCondition((ConditionExpression)condition);
                    }
                }

                filter.AddFilter(innerFilter);
            }

            var query = new QueryExpression(Contact.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(
                    Contact.Fields.dfeta_TRN,
                    Contact.Fields.EMailAddress1,
                    Contact.Fields.FirstName,
                    Contact.Fields.LastName,
                    Contact.Fields.BirthDate,
                    Contact.Fields.dfeta_NINumber,
                    Contact.Fields.dfeta_ActiveSanctions
                ),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions =
                    {
                        new ConditionExpression(Contact.Fields.StateCode, ConditionOperator.Equal, (int)ContactState.Active)
                    },
                    Filters =
                    {
                        filter
                    }
                },
                Orders =
                {
                    new OrderExpression(Contact.Fields.LastName, OrderType.Ascending),
                    new OrderExpression(Contact.Fields.FirstName, OrderType.Ascending)
                }
            };

            if (ittProviderLink != null)
            {
                query.LinkEntities.Add(ittProviderLink);
            }

            var result = await _service.RetrieveMultipleAsync(query);

            var contacts = result.Entities.Select(entity => entity.ToEntity<Contact>());

            // De-dup records (we might get multiple results for the same person because of the ITT provider join)
            return contacts.GroupBy(c => c.Id).Select(c => c.First()).ToArray();
        }

        public RequestBuilder CreateMultipleRequestBuilder() => RequestBuilder.CreateMultiple(_service);

        public RequestBuilder CreateSingleRequestBuilder() => RequestBuilder.CreateSingle(_service);

        public RequestBuilder CreateTransactionRequestBuilder() => RequestBuilder.CreateTransaction(_service);
    }
}
