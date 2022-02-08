using System;
using DqtApi.V2.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DqtApi.V2.Requests
{
    public class FindTeachersRequest : IRequest<FindTeachersResponse>
    {
        [SwaggerParameter(Description = "Email Address")]
        [FromQuery(Name = "emailAddress")]
        public string EmailAddress { get; set; }

        [SwaggerParameter(Description = "First name of person")]
        [FromQuery(Name = "firstName")]
        public string FirstName { get; set; }

        [SwaggerParameter(Description = "Middle name of person")]
        [FromQuery(Name = "middleName")]
        public string MiddleName { get; set; }

        [SwaggerParameter(Description = "Last name of person")]
        [FromQuery(Name = "lastName")]
        public string LastName { get; set; }

        [SwaggerParameter(Description = "Previous first name of person")]
        [FromQuery(Name = "previousFirstName")]
        public string PreviousFirstName { get; set; }

        [SwaggerParameter(Description = "Previous last name of person")]
        [FromQuery(Name = "previousLastName")]
        public string PreviousLastName { get; set; }

        [FromQuery(Name = "dateOfBirth")]
        [SwaggerParameter(Description = "Date of birth of person")]
        [ModelBinder(typeof(ModelBinding.DateModelBinder))]
        public DateOnly? DateOfBirth { get; set; }

        [SwaggerParameter(Description = "National insurance number of person")]
        [FromQuery(Name = "nationalInsuranceNumber")]
        public string NationalInsuranceNumber { get; set; }

        [SwaggerParameter(Description = "Name of teacher training provider")]
        [FromQuery(Name = "ittProviderName")]
        public string IttProviderName { get; set; }

        [SwaggerParameter(Description = "UKPRN of teacher training provider")]
        [FromQuery(Name = "ittProviderUkprn")]
        public string IttProviderUkprn { get; set; }
    }
}
