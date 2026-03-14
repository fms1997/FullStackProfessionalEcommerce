using FulSpectrum.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FulSpectrum.Infrastructure.Persistence.Configurations;

internal sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductNameSnapshot).HasMaxLength(160);
        builder.Property(x => x.SkuSnapshot).HasMaxLength(64);
        builder.HasCheckConstraint("CK_OrderItems_Quantity", "[Quantity] >= 1");
    }
}
