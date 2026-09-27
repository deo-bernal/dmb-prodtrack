using ProdTrack.Application.Messaging;
using ProdTrack.Application.Products.CreateProduct;
using ProdTrack.Application.Products.GetProduct;
using ProdTrack.Application.Products.GetProducts;
using ProdTrack.Application.Products.UpdateProduct;
using ProdTrack.Application.Security;
using ProdTrack.Contracts.Products;
using ProdTrack.Domain.Products;
using ProdTrack.Server.Errors;

namespace ProdTrack.Server.Endpoints;

internal static class ProductEndpoints
{
    public static RouteGroupBuilder MapProductEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/products").WithTags("Products");

        group.MapGet("/", async (IDispatcher dispatcher, string? productType, string? search, bool? includeInactive, CancellationToken ct) =>
            {
                var type = EnumBinding.ParseOptional<ProductType>(productType, "productType");
                if (type.IsFailure)
                {
                    return type.Error!.ToProblem();
                }

                return (await dispatcher.QueryAsync(new GetProductsQuery(type.Value, search, includeInactive ?? false), ct))
                    .ToHttp(items => TypedResults.Ok(items.Select(p => p.ToDto()).ToList()));
            })
            .RequireAuthorization(Policies.ReadAll)
            .Produces<List<ProductDto>>();

        group.MapGet("/{id:int}", async (HttpContext http, IDispatcher dispatcher, int id, CancellationToken ct) =>
                (await dispatcher.QueryAsync(new GetProductQuery(id), ct)).ToHttp(p =>
                {
                    ETags.Set(http.Response, p.Version);
                    return TypedResults.Ok(p.ToDto());
                }))
            .RequireAuthorization(Policies.ReadAll)
            .Produces<ProductDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (IDispatcher dispatcher, CreateProductRequest request, CancellationToken ct) =>
            {
                var type = EnumBinding.Parse<ProductType>(request.ProductType, "productType");
                if (type.IsFailure)
                {
                    return type.Error!.ToProblem();
                }

                var command = new CreateProductCommand(request.Sku, request.Name, type.Value, request.RequiresArtworkApproval, request.Spec.ToDomain());
                return (await dispatcher.SendAsync(command, ct)).ToHttp(p => TypedResults.Created($"/api/v1/products/{p.Id}", p.ToDto()));
            })
            .RequireAuthorization(Policies.ManageProductsAndRoutings)
            .Produces<ProductDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:int}", async (HttpContext http, IDispatcher dispatcher, int id, UpdateProductRequest request, CancellationToken ct) =>
            {
                var version = ETags.ReadVersion(http.Request);
                if (version.IsFailure)
                {
                    return version.Error!.ToProblem();
                }

                var command = new UpdateProductCommand(id, request.Name, request.RequiresArtworkApproval, request.Spec.ToDomain(), request.IsActive, version.Value);
                return (await dispatcher.SendAsync(command, ct)).ToHttp(p =>
                {
                    ETags.Set(http.Response, p.Version);
                    return TypedResults.Ok(p.ToDto());
                });
            })
            .RequireAuthorization(Policies.ManageProductsAndRoutings)
            .Produces<ProductDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status412PreconditionFailed);

        return api;
    }
}
