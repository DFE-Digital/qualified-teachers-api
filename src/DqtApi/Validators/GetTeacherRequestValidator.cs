﻿using DqtApi.Models;
using FluentValidation;

namespace DqtApi.Validators
{
    public class GetTeacherRequestValidator : AbstractValidator<GetTeacherRequest>
    {
        public GetTeacherRequestValidator()
        {
            RuleFor(x => x.TRN)
                .Matches(@"^\d{7}$")
                .WithMessage(Properties.StringResources.ErrorMessages_TRNMustBe7Digits);
        }
    }
}
