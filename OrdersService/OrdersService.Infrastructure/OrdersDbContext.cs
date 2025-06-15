using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using OrdersService.Domain.Entities;
using OrdersService.Application.Abstractions;

namespace OrdersService.Infrastructure;

public class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Order> Orders { get; set; }

    public DbSet<InboxState> InboxState { get; set; }
    public DbSet<OutboxMessage> OutboxMessage { get; set; }
    public DbSet<OutboxState> OutboxState { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdersDbContext).Assembly);
    }
}