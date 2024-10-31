using Auth_API.DTOs;
using FluentValidation;

namespace Auth_API.Validator
{
    public class CreateEndpointValidator : AbstractValidator<CreateEndpointRequest>
    {
        public CreateEndpointValidator()
        {
            RuleFor(request => request.Route)
                .NotEmpty()
                .WithMessage("All routes must be informed");

            RuleFor(request => request.HttpMethod)
                .IsInEnum()
                .WithMessage("All HTTP Methods must be valid");
        }
    }
}
