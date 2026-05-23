using SoftOne.Auth.Helpers;
using SoftOne.Auth.Interfaces;
using SoftOne.Auth.Models;

namespace SoftOne.Auth.Services;

public class AuthService : IAuthService
{
    public AuthResult ValidateCredentials(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return AuthResult.Failed("Username is required.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return AuthResult.Failed("Password is required.");
        }

        if (!string.Equals(username, AuthCredentials.Username, StringComparison.Ordinal))
        {
            return AuthResult.Failed("Invalid username or password.");
        }

        if (!PasswordHasher.VerifyPassword(password, AuthCredentials.PasswordHash))
        {
            return AuthResult.Failed("Invalid username or password.");
        }

        return AuthResult.Authenticated(AuthCredentials.Username);
    }
}
