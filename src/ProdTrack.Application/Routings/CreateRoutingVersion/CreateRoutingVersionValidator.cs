using FluentValidation;
using ProdTrack.Domain.Routings;

namespace ProdTrack.Application.Routings.CreateRoutingVersion;

internal sealed class CreateRoutingVersionValidator : AbstractValidator<CreateRoutingVersionCommand>
{
    public CreateRoutingVersionValidator()
    {
        RuleFor(c => c.ProductType).IsInEnum();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(Routing.NameMaxLength);
        RuleFor(c => c.Steps).NotEmpty();
        RuleForEach(c => c.Steps).ChildRules(step =>
        {
            step.RuleFor(s => s.StationCode).NotEmpty();
            step.RuleFor(s => s.Sequence).GreaterThan(0);
        });
    }
}
