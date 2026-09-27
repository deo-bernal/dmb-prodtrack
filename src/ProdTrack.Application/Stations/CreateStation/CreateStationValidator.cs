using FluentValidation;
using ProdTrack.Domain.Stations;

namespace ProdTrack.Application.Stations.CreateStation;

internal sealed class CreateStationValidator : AbstractValidator<CreateStationCommand>
{
    public CreateStationValidator()
    {
        RuleFor(c => c.Code).NotEmpty().MaximumLength(Station.CodeMaxLength);
        RuleFor(c => c.Name).NotEmpty().MaximumLength(Station.NameMaxLength);
        RuleFor(c => c.Type).IsInEnum();
        RuleFor(c => c.WorkCenter).MaximumLength(50);
    }
}
