using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentsService.Application.Abstractions;
using PaymentsService.Infrastructure.Repositories;

namespace PaymentsService.Infrastructure;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSQL");

        services.AddDbContext<PaymentsDbContext>(options =>
            options.UseNpgsql(connectionString));
        
        services.AddScoped<IAccountRepository, PostgresAccountRepository>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PaymentsDbContext>());

        return services;
    }
}