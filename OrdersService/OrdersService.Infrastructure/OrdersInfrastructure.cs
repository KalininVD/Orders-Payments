using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace OrdersService.Infrastructure;

public static class OrdersInfrastructure
{
    public static void Migrate(IServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<OrdersDbContext>().Database.Migrate();
    }
}