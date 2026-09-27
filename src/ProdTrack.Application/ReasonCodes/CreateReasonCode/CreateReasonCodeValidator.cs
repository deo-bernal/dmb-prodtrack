using FluentValidation;
using ProdTrack.Domain.ReasonCodes;

namespace ProdTrack.Application.ReasonCodes.CreateReasonCode;

internal sealed class CreateReasonCodeValidator : AbstractValidator<CreateReasonCodeCommand>
{
    public CreateReasonCodeValidator()
    {
        RuleFor(c => c.Code).NotEmpty().MaximumLength(ReasonCode.CodeMaxLength);
        RuleFor(c => c.Description).NotEmpty().MaximumLength(ReasonCode.DescriptionMaxLength);
        RuleFor(c => c.Category).IsInEnum();
    }
}
