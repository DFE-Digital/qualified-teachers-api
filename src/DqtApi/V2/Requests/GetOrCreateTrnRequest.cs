using System;
using System.ComponentModel.DataAnnotations;
using DqtApi.V2.ApiModels;
using DqtApi.V2.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DqtApi.V2.Requests
{
    public class GetOrCreateTrnRequest : IRequest<TrnRequestInfo>
    {
        [FromRoute(Name = "requestId")]
        [SwaggerParameter(Description =
            "A unique ID that represents this request. " +
            "If a request has already been created with this ID then that existing record's result is returned.")]
        public string RequestId { get; set; }

        [Required]
        public string FirstName { get; set; }
        [Required]
        public string MiddleName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public DateOnly BirthDate { get; set; }
        [Required]
        public string EmailAddress { get; set; }
        public GetOrCreateTrnRequestAddress Address { get; set; }
        [Required]
        public Gender GenderCode { get; set; }
        [Required]
        public GetOrCreateTrnRequestInitialTeacherTraining InitialTeacherTraining { get; set; }
        [Required]
        public GetOrCreateTrnRequestQualification Qualification { get; set; }
    }

    public class GetOrCreateTrnRequestAddress
    {
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
    }

    public class GetOrCreateTrnRequestInitialTeacherTraining
    {
        [Required]
        public string ProviderUkprn { get; set; }
        [Required]
        public DateTime ProgrammeStartDate { get; set; }
        [Required]
        public DateTime ProgrammeEndDate { get; set; }
        [Required]
        public IttProgrammeType ProgrammeType { get; set; }
        [Required]
        public string Subject1 { get; set; }
        [Required]
        public string Subject2 { get; set; }
        [Required]
        public IttResult Result { get; set; }
    }

    public class GetOrCreateTrnRequestQualification
    {
        [Required]
        public string ProviderUkprn { get; set; }
        [Required]
        public string CountryCode { get; set; }
        [Required]
        public string Subject { get; set; }
        [Required]
        public ClassDivision Class { get; set; }
        [Required]
        public DateTime Date { get; set; }
    }
}
