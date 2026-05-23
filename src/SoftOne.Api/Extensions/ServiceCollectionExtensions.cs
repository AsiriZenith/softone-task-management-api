using SoftOne.Api.Configuration;
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
        services.Configure<AppSettings>(configuration.GetSection(AppSettings.SectionName));

        services.AddDatabase(configuration);

        // TODO Phase 6: register FluentValidation validators.
        // services.AddValidatorsFromAssemblyContaining<Program>();

        // TODO Phase 3: wire authentication services from SoftOne.Auth.
        services.AddScoped<IAuthService, AuthService>();

        // TODO Phase 7: implement task service business logic.
        services.AddScoped<ITaskService, TaskService>();

        return services;
    }
}
