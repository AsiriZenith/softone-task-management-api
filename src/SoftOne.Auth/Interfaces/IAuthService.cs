using SoftOne.Auth.Models;

namespace SoftOne.Auth.Interfaces;

/// <summary>
/// Validates hardcoded credentials using BCrypt. Does not issue tokens.
/// </summary>
public interface IAuthService
{
    Task<AuthResult> ValidateCredentialsAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default);
}
