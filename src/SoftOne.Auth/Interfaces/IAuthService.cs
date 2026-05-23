using SoftOne.Auth.Models;

namespace SoftOne.Auth.Interfaces;

public interface IAuthService
{
    AuthResult ValidateCredentials(string username, string password);
}
