using FulSpectrum.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FulSpectrum.Infrastructure.Persistence.Configurations;

internal sealed class PaymentWebhookLogConfiguration : IEntityTypeConfiguration<PaymentWebhookLog>
{
    public void Configure(EntityTypeBuilder<PaymentWebhookLog> builder)
    {
        builder.ToTable("PaymentWebhookLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Provider).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.ProviderEventId).HasMaxLength(128);
        builder.Property(x => x.EventType).HasMaxLength(128);
        builder.Property(x => x.PayloadRaw).HasColumnType("nvarchar(max)");
        builder.Property(x => x.ProcessingResult).HasMaxLength(64);
        builder.Property(x => x.Error).HasMaxLength(1024);

        builder.HasIndex(x => new { x.Provider, x.ProviderEventId }).IsUnique();
    }
}
