using PaymentsService.Application;
using PaymentsService.Infrastructure;
using PaymentsService.Web.Middleware;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Payments Service API",
        Description = "API для управления счетами и платежами клиентов"
    });

    options.CustomSchemaIds(type => type.FullName);
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();

app.MapControllers();
app.UseHttpsRedirection();

PaymentsInfrastructure.Migrate(app.Services);

app.Run();