var builder = WebApplication.CreateBuilder(args);

var reverseProxySection = builder.Configuration.GetSection("ReverseProxy");

void ReplaceEnvVarsInSection(IConfigurationSection section)
{
    foreach (var child in section.GetChildren())
    {
        if (child.Value != null)
        {
            var newValue = Environment.ExpandEnvironmentVariables(child.Value);
            builder.Configuration[$"{child.Path}"] = newValue;
        }
        else
        {
            ReplaceEnvVarsInSection(child);
        }
    }
}

ReplaceEnvVarsInSection(reverseProxySection);

builder.Services.AddReverseProxy().LoadFromConfig(reverseProxySection);

var app = builder.Build();

app.MapReverseProxy();

app.Run();