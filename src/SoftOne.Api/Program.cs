using SoftOne.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

app.ConfigureMiddlewarePipeline();
app.MapApplicationEndpoints();

app.Run();

public partial class Program;
