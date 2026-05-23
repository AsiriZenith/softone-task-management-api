using SoftOne.Auth.Interfaces;
using SoftOne.Auth.Models;

namespace SoftOne.Auth.Services;

public class AuthService : IAuthService
{
    public Task<AuthResult> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
