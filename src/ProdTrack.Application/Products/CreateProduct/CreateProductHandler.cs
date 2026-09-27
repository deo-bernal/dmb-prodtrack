using Microsoft.EntityFrameworkCore;
using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.Products;

namespace ProdTrack.Application.Products.CreateProduct;

internal sealed class CreateProductHandler(IAppDbContext db) : ICommandHandler<CreateProductCommand, ProductModel>
{
    public async Task<Result<ProductModel>> HandleAsync(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var created = Product.Create(command.Sku, command.Name, command.ProductType, command.RequiresArtworkApproval, command.Spec);
        if (created.IsFailure)
        {
            return created.Error!;
        }

        var product = created.Value;
        var referenceError = await ProductReferenceCheck.CheckAsync(db, product.DefaultSpec, cancellationToken);
        if (referenceError is not null)
        {
            return referenceError;
        }

        if (await db.Products.AnyAsync(p => p.Sku == product.Sku, cancellationToken))
        {
            return ProductErrors.DuplicateSku(product.Sku);
        }

        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken);
        return ProductModel.From(product);
    }
}
