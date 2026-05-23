using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SoftOne.Api.Configuration;
using SoftOne.Api.Data;
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

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddValidatorsFromAssemblyContaining<Program>();

        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new()
            {
                Title = "SoftOne API",
                Version = "v1",
                Description = "Task Management API"
            });
        });

        return services;
    }
}
