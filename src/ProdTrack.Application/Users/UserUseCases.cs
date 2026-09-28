using FluentValidation;
using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;
using ProdTrack.Domain.Common;

namespace ProdTrack.Application.Users;

[RequiresPolicy(Policies.ManageUsers)]
public sealed record GetUsersQuery : IQuery<IReadOnlyList<UserSummaryModel>>;

[RequiresPolicy(Policies.ManageUsers)]
public sealed record GetUserQuery(string Id) : IQuery<UserDetailModel>;

/// <summary>Admin creates a user with a temporary password; the user must change it at first sign-in (PT-012).</summary>
[RequiresPolicy(Policies.ManageUsers)]
public sealed record CreateUserCommand(
    string Email,
    string DisplayName,
    string TemporaryPassword,
    IReadOnlyList<string> Roles,
    string? EmployeeNumber = null,
    string? BadgeNumber = null,
    string? HomeStationCode = null) : ICommand<UserCreatedModel>;

[RequiresPolicy(Policies.ManageUsers)]
public sealed record UpdateUserCommand(
    string Id,
    string DisplayName,
    IReadOnlyList<string> Roles,
    string? EmployeeNumber,
    string? BadgeNumber,
    string? HomeStationCode,
    string? ExpectedVersion = null) : ICommand<UserDetailModel>;

[RequiresPolicy(Policies.ManageUsers)]
public sealed record SetUserActiveCommand(string Id, bool IsActive) : ICommand<Unit>;

[RequiresPolicy(Policies.ManageUsers)]
public sealed record ResetUserPasswordCommand(string Id, string TemporaryPassword) : ICommand<Unit>;

internal static class UserRules
{
    public const int MinPasswordLength = 12;

    public static IRuleBuilderOptions<T, IReadOnlyList<string>> ValidRoles<T>(this IRuleBuilder<T, IReadOnlyList<string>> rule) =>
        rule.NotEmpty().WithMessage("Choose at least one role.")
            .Must(roles => roles.All(r => Roles.All.Contains(r, StringComparer.Ordinal))).WithMessage("Unknown role.");
}

internal sealed class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(c => c.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(c => c.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(c => c.TemporaryPassword).NotEmpty().MinimumLength(UserRules.MinPasswordLength);
        RuleFor(c => c.Roles).ValidRoles();
        RuleFor(c => c.EmployeeNumber).MaximumLength(20);
        RuleFor(c => c.BadgeNumber).MaximumLength(20);
    }
}

internal sealed class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserValidator()
    {
        RuleFor(c => c.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Roles).ValidRoles();
        RuleFor(c => c.EmployeeNumber).MaximumLength(20);
        RuleFor(c => c.BadgeNumber).MaximumLength(20);
    }
}

internal sealed class ResetUserPasswordValidator : AbstractValidator<ResetUserPasswordCommand>
{
    public ResetUserPasswordValidator() => RuleFor(c => c.TemporaryPassword).NotEmpty().MinimumLength(UserRules.MinPasswordLength);
}

internal sealed class UserUseCasesHandler(IUserAdministration users, ICurrentUser currentUser)
    : IQueryHandler<GetUsersQuery, IReadOnlyList<UserSummaryModel>>,
      IQueryHandler<GetUserQuery, UserDetailModel>,
      ICommandHandler<CreateUserCommand, UserCreatedModel>,
      ICommandHandler<UpdateUserCommand, UserDetailModel>,
      ICommandHandler<SetUserActiveCommand, Unit>,
      ICommandHandler<ResetUserPasswordCommand, Unit>
{
    public async Task<Result<IReadOnlyList<UserSummaryModel>>> HandleAsync(GetUsersQuery query, CancellationToken cancellationToken) =>
        Result.Success(await users.ListAsync(cancellationToken));

    public async Task<Result<UserDetailModel>> HandleAsync(GetUserQuery query, CancellationToken cancellationToken)
    {
        var user = await users.GetAsync(query.Id, cancellationToken);
        return user is null ? UserErrors.NotFound(query.Id) : user;
    }

    public async Task<Result<UserCreatedModel>> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var created = await users.CreateAsync(
            new NewUser(command.Email, command.DisplayName, command.TemporaryPassword, command.Roles, command.EmployeeNumber, command.BadgeNumber, command.HomeStationCode),
            cancellationToken);
        return created.IsSuccess ? new UserCreatedModel(created.Value) : created.Error!;
    }

    public async Task<Result<UserDetailModel>> HandleAsync(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        if (string.Equals(command.Id, currentUser.UserId, StringComparison.Ordinal) && !command.Roles.Contains(Roles.Admin, StringComparer.Ordinal))
        {
            return UserErrors.CannotRemoveOwnAdmin;
        }

        var result = await users.UpdateAsync(
            command.Id,
            new UserProfileUpdate(command.DisplayName, command.Roles, command.EmployeeNumber, command.BadgeNumber, command.HomeStationCode),
            command.ExpectedVersion,
            cancellationToken);
        if (result.IsFailure)
        {
            return result.Error!;
        }

        return (await users.GetAsync(command.Id, cancellationToken))!;
    }

    public async Task<Result<Unit>> HandleAsync(SetUserActiveCommand command, CancellationToken cancellationToken)
    {
        if (!command.IsActive && string.Equals(command.Id, currentUser.UserId, StringComparison.Ordinal))
        {
            return UserErrors.CannotDeactivateSelf;
        }

        var result = await users.SetActiveAsync(command.Id, command.IsActive, cancellationToken);
        return result.IsSuccess ? Unit.Value : result.Error!;
    }

    public async Task<Result<Unit>> HandleAsync(ResetUserPasswordCommand command, CancellationToken cancellationToken)
    {
        var result = await users.ResetPasswordAsync(command.Id, command.TemporaryPassword, cancellationToken);
        return result.IsSuccess ? Unit.Value : result.Error!;
    }
}
