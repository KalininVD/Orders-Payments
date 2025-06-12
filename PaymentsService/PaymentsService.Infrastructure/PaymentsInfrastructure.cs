using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace PaymentsService.Infrastructure;

public static class PaymentsInfrastructure
{
    public static void Migrate(IServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<PaymentsDbContext>().Database.Migrate();
    }
}