using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;
using ProdTrack.Application.Users;
using ProdTrack.Contracts.Users;
using ProdTrack.Server.Errors;

namespace ProdTrack.Server.Endpoints;

/// <summary>User administration (PT-012), Admin only.</summary>
internal static class UserEndpoints
{
    public static RouteGroupBuilder MapUserEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/users").WithTags("Users").RequireAuthorization(Policies.ManageUsers);

        group.MapGet("/", async (IDispatcher dispatcher, CancellationToken ct) =>
                (await dispatcher.QueryAsync(new GetUsersQuery(), ct)).ToHttp(u => TypedResults.Ok(u.Select(x => x.ToDto()).ToList())))
            .Produces<List<UserSummaryDto>>();

        group.MapGet("/{id}", async (HttpContext http, IDispatcher dispatcher, string id, CancellationToken ct) =>
                (await dispatcher.QueryAsync(new GetUserQuery(id), ct)).ToHttp(u =>
                {
                    ETags.SetRaw(http.Response, u.Version);
                    return TypedResults.Ok(u.ToDto());
                }))
            .Produces<UserDetailDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (IDispatcher dispatcher, CreateUserRequest request, CancellationToken ct) =>
                (await dispatcher.SendAsync(
                    new CreateUserCommand(request.Email, request.DisplayName, request.TemporaryPassword, request.Roles ?? [], request.EmployeeNumber, request.BadgeNumber, request.HomeStationCode), ct))
                    .ToHttp(u => TypedResults.Created($"/api/v1/users/{u.Id}", new UserCreatedResponse(u.Id))))
            .WithSummary("Create a user with a temporary password (must change it at first sign-in)")
            .Produces<UserCreatedResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{id}", async (HttpContext http, IDispatcher dispatcher, string id, UpdateUserRequest request, CancellationToken ct) =>
                (await dispatcher.SendAsync(
                    new UpdateUserCommand(id, request.DisplayName, request.Roles ?? [], request.EmployeeNumber, request.BadgeNumber, request.HomeStationCode, ETags.ReadRaw(http.Request)), ct))
                    .ToHttp(u =>
                    {
                        ETags.SetRaw(http.Response, u.Version);
                        return TypedResults.Ok(u.ToDto());
                    }))
            .WithSummary("Update profile and roles. Optional If-Match (concurrency stamp).")
            .Produces<UserDetailDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status412PreconditionFailed);

        group.MapPost("/{id}/active", async (IDispatcher dispatcher, string id, SetUserActiveRequest request, CancellationToken ct) =>
                (await dispatcher.SendAsync(new SetUserActiveCommand(id, request.IsActive), ct)).ToHttp(_ => TypedResults.NoContent()))
            .WithSummary("Enable or disable a user (disabled users cannot sign in; sessions end at the next revalidation)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id}/reset-password", async (IDispatcher dispatcher, string id, ResetPasswordRequest request, CancellationToken ct) =>
                (await dispatcher.SendAsync(new ResetUserPasswordCommand(id, request.TemporaryPassword), ct)).ToHttp(_ => TypedResults.NoContent()))
            .WithSummary("Set a temporary password; the user must change it at next sign-in")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        return api;
    }
}
