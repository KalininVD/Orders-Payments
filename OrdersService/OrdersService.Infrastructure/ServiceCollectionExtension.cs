using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using OrdersService.Application.Abstractions;
using OrdersService.Infrastructure.Repositories;
using OrdersService.Infrastructure.Options;
using OrdersService.Application.UseCases.Consumers;
using OrdersService.Infrastructure.Notifications;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace OrdersService.Infrastructure;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<RabbitMqOptions>()
            .BindConfiguration(RabbitMqOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var connectionString = configuration.GetConnectionString("PostgreSQL");

        services.AddDbContext<OrdersDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IOrderRepository, PostgresOrderRepository>();

        services.AddHttpClient();

        var apiGatewayConfig = configuration.GetRequiredSection("ApiGateway");

        services.AddScoped<IOrderNotifier>(sp =>
        {
            var baseUrl = apiGatewayConfig["BaseUrl"]
                ?? throw new InvalidOperationException("ApiGateway:BaseUrl is not configured.");

            var notifyEndpoint = apiGatewayConfig["NotifyEndpoint"]
                ?? throw new InvalidOperationException("ApiGateway:NotifyEndpoint is not configured.");

            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();

            var logger = sp.GetRequiredService<ILogger<ApiGatewayNotifier>>();

            return new ApiGatewayNotifier(httpClientFactory, baseUrl, notifyEndpoint, logger);
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<OrdersDbContext>());

        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.AddConsumer<OrderPaymentSucceededConsumer>();
            busConfigurator.AddConsumer<OrderPaymentFailedConsumer>();

            busConfigurator.AddEntityFrameworkOutbox<OrdersDbContext>(outboxConfigurator =>
            {
                outboxConfigurator.UsePostgres();
                outboxConfigurator.UseBusOutbox();
            });

            busConfigurator.SetKebabCaseEndpointNameFormatter();

            busConfigurator.UsingRabbitMq((context, mqConfigurator) =>
            {
                var options = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;

                mqConfigurator.Host(options.Host, "/", h =>
                {
                    h.Username(options.User);
                    h.Password(options.Password);
                });

                mqConfigurator.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}