using SoftOne.Api.Middleware;

namespace SoftOne.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication ConfigureMiddlewarePipeline(this WebApplication app)
    {
        app.UseSwaggerDocumentation();

        if (!app.Environment.IsEnvironment("Testing"))
        {
            app.UseHttpsRedirection();
        }

        // TODO Phase 4: implement global exception handling behavior.
        app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

        // TODO Phase 4: implement authentication middleware behavior.
        app.UseMiddleware<AuthenticationMiddleware>();

        return app;
    }
}
