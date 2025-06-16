using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using PaymentsService.Application.Abstractions;
using PaymentsService.Infrastructure.Repositories;
using PaymentsService.Infrastructure.Options;
using PaymentsService.Application.UseCases.Consumers;
using PaymentsService.Infrastructure.Services;

namespace PaymentsService.Infrastructure;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<RabbitMqOptions>()
            .BindConfiguration(RabbitMqOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var connectionString = configuration.GetConnectionString("PostgreSQL");

        services.AddDbContext<PaymentsDbContext>(options =>
            options.UseNpgsql(connectionString));
        
        services.AddScoped<IAccountRepository, PostgresAccountRepository>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PaymentsDbContext>());

        services.AddSingleton<IDelayProvider, DefaultDelayProvider>();

        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.AddConsumer<OrderPaymentRequestConsumer>();

            busConfigurator.AddEntityFrameworkOutbox<PaymentsDbContext>(outboxConfigurator =>
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