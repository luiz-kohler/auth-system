using Auth_API.DTOs;
using FluentValidation;

namespace Auth_API.Validator
{
    public class CreateProjectValidator : AbstractValidator<CreateProjectRequest>
    {
        public CreateProjectValidator()
        {
            RuleFor(request => request.Name)
                .NotEmpty()
                .WithMessage("Name must be informed");

            RuleFor(request => request.Endpoints)
                .NotNull()
                .WithMessage("Endpoints must be informed");
        }
    }
}
