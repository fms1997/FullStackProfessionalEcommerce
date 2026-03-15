using FulSpectrum.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FulSpectrum.Infrastructure.Persistence.Configurations;

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Provider).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.ProviderPaymentId).HasMaxLength(128);
        builder.Property(x => x.ExternalReference).HasMaxLength(128);
        builder.Property(x => x.Currency).HasMaxLength(8);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(24);
        builder.Property(x => x.FailureCode).HasMaxLength(64);
        builder.Property(x => x.FailureMessage).HasMaxLength(512);

        builder.HasIndex(x => new { x.Provider, x.ProviderPaymentId });
        builder.HasIndex(x => new { x.OrderId, x.Provider, x.ExternalReference }).IsUnique();

        builder.HasOne(x => x.Order)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
