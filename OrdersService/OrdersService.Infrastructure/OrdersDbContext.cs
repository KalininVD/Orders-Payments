using Microsoft.EntityFrameworkCore;
// using OrdersService.Domain.Entities;

namespace OrdersService.Infrastructure;

public class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
{
    // public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdersDbContext).Assembly);
        modelBuilder.HasDefaultSchema("orders");
        base.OnModelCreating(modelBuilder);
    }
}