﻿using System;

namespace DqtApi.DataStore.Crm
{
    public class FindTeachersQuery
    {
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string PreviousFirstName { get; set; }
        public string PreviousLastName { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string NationalInsuranceNumber { get; set; }
        public Guid? IttProviderOrganizationId { get; set; }
    }
}