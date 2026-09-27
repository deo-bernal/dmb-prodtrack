using FluentValidation;
using ProdTrack.Domain.Products;

namespace ProdTrack.Application.Products.CreateProduct;

internal sealed class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(c => c.Sku).NotEmpty().MaximumLength(Product.SkuMaxLength);
        RuleFor(c => c.Name).NotEmpty().MaximumLength(Product.NameMaxLength);
        RuleFor(c => c.ProductType).IsInEnum();
        RuleFor(c => c.Spec).NotNull();
    }
}
