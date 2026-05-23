using SoftOne.Auth.Interfaces;
using SoftOne.Auth.Models;

namespace SoftOne.Auth.Services;

/// <summary>
/// Hardcoded credential validation with BCrypt password verification. No token generation.
/// </summary>
public class AuthService : IAuthService
{
    public Task<AuthResult> ValidateCredentialsAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
