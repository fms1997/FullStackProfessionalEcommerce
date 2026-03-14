using FulSpectrum.Domain.Catalog;
using FulSpectrum.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FulSpectrum.Infrastructure.Persistence.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Currency).HasMaxLength(8);
        builder.Property(x => x.ShippingFullName).HasMaxLength(120);
        builder.Property(x => x.ShippingAddressLine1).HasMaxLength(160);
        builder.Property(x => x.ShippingAddressLine2).HasMaxLength(160);
        builder.Property(x => x.ShippingCity).HasMaxLength(80);
        builder.Property(x => x.ShippingState).HasMaxLength(80);
        builder.Property(x => x.ShippingPostalCode).HasMaxLength(20);
        builder.Property(x => x.ShippingCountryCode).HasMaxLength(2);

        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(24);
        builder.HasIndex(x => new { x.UserId, x.CreatedAtUtc });

        builder.HasMany(x => x.Items)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
