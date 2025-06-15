using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrdersService.Domain.Entities;

namespace OrdersService.Infrastructure.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Amount)
            .HasColumnType("decimal(16, 2)");

        builder.Property(o => o.Description)
            .HasMaxLength(512);

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(64);
    }
}