namespace SoftOne.Api.Endpoints;

public static class EndpointExtensions
{
    public static WebApplication MapApplicationEndpoints(this WebApplication app)
    {
        app.MapTaskEndpoints();
        return app;
    }
}
