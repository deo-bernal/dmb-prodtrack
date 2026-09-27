namespace ProdTrack.Application.Abstractions;

/// <summary>Evaluates a named policy for the current user (implemented by the Server with IAuthorizationService).</summary>
public interface IAuthorizationChecker
{
    Task<bool> IsAuthorizedAsync(string policy, CancellationToken cancellationToken);
}
