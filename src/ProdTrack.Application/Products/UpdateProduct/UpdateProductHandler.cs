using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.Products;

namespace ProdTrack.Application.Products.UpdateProduct;

internal sealed class UpdateProductHandler(IAppDbContext db) : ICommandHandler<UpdateProductCommand, ProductModel>
{
    public async Task<Result<ProductModel>> HandleAsync(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var product = await db.Products.FindAsync([command.Id], cancellationToken);
        if (product is null)
        {
            return ProductErrors.NotFound(command.Id);
        }

        if (!ConcurrencyErrors.Matches(product.RowVersion, command.ExpectedVersion))
        {
            return ConcurrencyErrors.StaleVersion;
        }

        var referenceError = await ProductReferenceCheck.CheckAsync(db, command.Spec, cancellationToken);
        if (referenceError is not null)
        {
            return referenceError;
        }

        var result = product.Update(command.Name, command.RequiresArtworkApproval, command.Spec, command.IsActive);
        if (result.IsFailure)
        {
            return result.Error!;
        }

        await db.SaveChangesAsync(cancellationToken);
        return ProductModel.From(product);
    }
}
