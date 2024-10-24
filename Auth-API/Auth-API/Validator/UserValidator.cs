using Auth_API.DTOs;
using FluentValidation;

namespace Auth_API.Validator
{
    public class CreateUserValidator : AbstractValidator<CreateUserRequest>
    {
        public CreateUserValidator()
        {
            RuleFor(request => request.Name)
                .NotEmpty()
                .WithMessage("Name must be informed");

            RuleFor(request => request.Email)
                .NotEmpty()
                .WithMessage("Email must be informed.")
                .EmailAddress()
                .WithMessage("Email must be valid");

            RuleFor(request => request.Password)
                .NotEmpty()
                .WithMessage("Password must be informed")
                .MinimumLength(8)
                .WithMessage("Password should have at least 8 characters.");
        }
    }
}
