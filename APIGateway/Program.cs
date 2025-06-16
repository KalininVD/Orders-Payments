using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var reverseProxyConfig = builder.Configuration.GetSection("ReverseProxy");

builder.Services.AddReverseProxy()
    .LoadFromConfig(reverseProxyConfig);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Gateway", Version = "v1" });
});

var app = builder.Build();

app.MapReverseProxy();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        var endpoints = app.Configuration.GetSection("ReverseProxy:Swagger:Endpoints").GetChildren();

        foreach (var endpoint in endpoints)
        {
            var name = endpoint.GetValue<string>("Name");
            var path = endpoint.GetValue<string>("Path");

            c.SwaggerEndpoint(path, name);
        }
    });
}

app.Run();