using Microsoft.EntityFrameworkCore;
// using PaymentsService.Domain.Entities;

namespace PaymentsService.Infrastructure;

public class PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : DbContext(options)
{
    // public DbSet<Payment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentsDbContext).Assembly);
        modelBuilder.HasDefaultSchema("orders");
        base.OnModelCreating(modelBuilder);
    }
}