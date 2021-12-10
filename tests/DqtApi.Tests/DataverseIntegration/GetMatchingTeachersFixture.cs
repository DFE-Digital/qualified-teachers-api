﻿using System;
using System.Linq;
using DqtApi.Models;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace DqtApi.Tests.DataverseIntegration
{
    public class GetMatchingTeachersFixture : CrmClientFixture
    {
        public struct Fixture
        {
            public string NationalInsuranceNumber { get; set; }
            public string TRN { get; set; }

            public Guid ID { get; set; }
        }

        public IOrganizationServiceAsync Service => ServiceClient;
        private readonly string _nonmatchingNationalInsuranceNumber;
        private readonly string _nonmatchingTRN;

        private readonly Fixture[] _fixtures;

        private static DateTime MatchingBirthdate => new(2001, 1, 1);
        private static DateTime NonmatchingBirthdate => new(2002, 2, 2);

        public GetMatchingTeachersFixture()
        {
            var nationalInsuranceNumberGenerator = new NationalInsuranceNumberGenerator(Service);

            var nationalInsuranceNumber1 = nationalInsuranceNumberGenerator.GetNextAvailable();
            var nationalInsuranceNumber2 = nationalInsuranceNumberGenerator.GetNextAvailable(nationalInsuranceNumber1);
            _nonmatchingNationalInsuranceNumber = nationalInsuranceNumberGenerator.GetNextAvailable(nationalInsuranceNumber2);

            var id1 = CreateContact(nationalInsuranceNumber1);
            var id2 = CreateContact(nationalInsuranceNumber2);

            var trn1 = GetTRN(id1);
            var trn2 = GetTRN(id2);
            _nonmatchingTRN = (int.Parse(trn2) + 1).ToString();

            _fixtures = new[]
            {
                new Fixture { ID = id1, TRN = trn1, NationalInsuranceNumber = nationalInsuranceNumber1 },
                new Fixture { ID = id2, TRN = trn2, NationalInsuranceNumber = nationalInsuranceNumber2 }
            };
        }

        private string GetTRN(Guid contactId)
        {
            return Service.Retrieve(Contact.EntityLogicalName, contactId, new ColumnSet(Contact.Fields.dfeta_TRN))
                .GetAttributeValue<string>(Contact.Fields.dfeta_TRN);
        }

        private Guid CreateContact(string nationalInsuranceNumber)
        {
            var id = Service.Create(new Contact
            {
                dfeta_NINumber = nationalInsuranceNumber,
                BirthDate = MatchingBirthdate
            });

            // updated TRN Required, so that TRN is generated
            Service.Update(new Contact
            {
                Id = id,
                dfeta_trnrequired = true
            });

            return id;
        }

        public enum MatchFixture
        {
            One,
            Two,
            None
        }

        public GetTeacherRequest GetRequest(MatchFixture matchingTRN, bool matchingBirthdate, MatchFixture? matchingNationalInsuranceNumber = null) => new()
        {
            TRN = matchingTRN switch
            {
                MatchFixture.One => _fixtures[0].TRN,
                MatchFixture.Two => _fixtures[1].TRN,
                _ => _nonmatchingTRN
            },
            BirthDate = matchingBirthdate ? MatchingBirthdate : NonmatchingBirthdate,
            NationalInsuranceNumber =  matchingNationalInsuranceNumber.HasValue ? matchingNationalInsuranceNumber switch
            {
                MatchFixture.One => _fixtures[0].NationalInsuranceNumber,
                MatchFixture.Two => _fixtures[1].NationalInsuranceNumber,
                _ => _nonmatchingNationalInsuranceNumber
            } : _nonmatchingNationalInsuranceNumber
        };

        public void AssertMatchesFixture(Contact teacher, int index)
        {
            Assert.Equal(_fixtures[index].ID, teacher.Id);
        }

        public override void Dispose()
        {
            // remove National Insurance Number from each fixture so it can be re-used
            Enumerable.Range(0, 2).ToList().ForEach(i =>
            {
                Service.Update(new Contact { Id = _fixtures[i].ID, dfeta_NINumber = string.Empty });
            });

            base.Dispose();
        }
    }
}