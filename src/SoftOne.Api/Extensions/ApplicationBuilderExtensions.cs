using SoftOne.Api.Middleware;

namespace SoftOne.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication ConfigureMiddlewarePipeline(this WebApplication app)
    {
        app.UseSwaggerDocumentation();

        if (app.Environment.IsProduction())
        {
            app.UseHttpsRedirection();
        }

        app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
        app.UseCors(CorsExtensions.FrontendPolicy);
        app.UseMiddleware<AuthenticationMiddleware>();

        return app;
    }
}
