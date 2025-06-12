using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PaymentsService.Infrastructure;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSQL");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'PostgreSQL' is missing.");
        }

        connectionString = Environment.ExpandEnvironmentVariables(connectionString);

        services.AddDbContext<PaymentsDbContext>(
            options => options.UseNpgsql(connectionString));

        return services;
    }
}