﻿using BurgerRoyale.Domain.DTO;
using FluentValidation;

namespace BurgerRoyale.API.Validators
{
    public class UserDTOValidator : AbstractValidator<UserDTO>
    {
        public UserDTOValidator()
        {
            When(w => w is not null, () => 
            {
                RuleFor(r => r.Cpf).NotNull().NotEmpty().WithMessage("Preencha o CPF!");
                RuleFor(r => r.Email).NotNull().NotEmpty().EmailAddress();
                RuleFor(r => r.UserType).NotNull().NotEmpty().IsInEnum();
            });
        }
    }
}