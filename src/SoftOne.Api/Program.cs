using System.Text.Json.Serialization;
using SoftOne.Api.Endpoints;
using SoftOne.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services
    .AddApplicationServices(builder.Configuration)
    .AddFrontendCors(builder.Configuration)
    .AddSwaggerDocumentation();

var app = builder.Build();

app.ConfigureMiddlewarePipeline();
app.MapApplicationEndpoints();

app.Run();

public partial class Program;
