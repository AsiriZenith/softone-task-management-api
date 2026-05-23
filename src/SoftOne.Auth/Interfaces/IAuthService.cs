using SoftOne.Auth.Models;

namespace SoftOne.Auth.Interfaces;

public interface IAuthService
{
    Task<AuthResult> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default);
}
