using SoftOne.Api.Services;
using SoftOne.Auth.Interfaces;
using SoftOne.Auth.Services;

namespace SoftOne.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDatabase(configuration);
        services.AddValidation();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITaskService, TaskService>();

        return services;
    }
}
