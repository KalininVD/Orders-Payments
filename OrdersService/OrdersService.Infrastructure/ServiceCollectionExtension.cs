using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrdersService.Application.Abstractions;
using OrdersService.Infrastructure.MessageBroker;
using MassTransit;

namespace OrdersService.Infrastructure;

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

        services.AddDbContext<OrdersDbContext>(
            options => options.UseNpgsql(connectionString));
        
        services.AddScoped<IEventBus, EventBus>();

        services.AddMassTransit(busConfigurator =>
        {
            // Здесь регистрируются все твои консьюмеры из этого сервиса
            // busConfigurator.AddConsumer<MyConsumer>();

            busConfigurator.UsingRabbitMq((context, mqConfigurator) =>
            {
                mqConfigurator.Host(configuration["RABBITMQ_HOST"], "/", h =>
                {
                    h.Username(configuration["RABBITMQ_USER"]);
                    h.Password(configuration["RABBITMQ_PASSWORD"]);
                });

                // Эта строка автоматически настраивает эндпоинты для всех
                // зарегистрированных консьюмеров. Магия!
                mqConfigurator.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}