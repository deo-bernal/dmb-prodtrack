using FluentValidation;

namespace ProdTrack.Application.WorkOrders.CreateWorkOrder;

internal sealed class CreateWorkOrderValidator : AbstractValidator<CreateWorkOrderCommand>
{
    public CreateWorkOrderValidator()
    {
        RuleFor(c => c.ProductId).GreaterThan(0);
        RuleFor(c => c.Quantity).GreaterThan(0);
        RuleFor(c => c.Priority).InclusiveBetween(1, 5);
    }
}
