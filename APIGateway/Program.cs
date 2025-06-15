using Yarp.ReverseProxy.Swagger.Extensions;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var reverseProxyConfig = builder.Configuration.GetSection("ReverseProxy");

builder.Services.AddReverseProxy()
    .LoadFromConfig(reverseProxyConfig)
    .AddSwagger(reverseProxyConfig);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Gateway", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        foreach (var endpoint in app.Configuration.GetSection("ReverseProxy:Swagger:Endpoints").GetChildren())
        {
            var key = endpoint.GetValue<string>("Key");
            var name = endpoint.GetValue<string>("Name");

            c.SwaggerEndpoint($"/swagger/{key}/swagger.json", name);
        }
    });
}

app.MapReverseProxy();

app.Run();