namespace FulSpectrum.Infrastructure.Persistence.Seeds;

internal static class SeedData
{
    public static readonly DateTime SeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static readonly Guid ElectronicsCategoryId = Guid.Parse("30d0f5fa-c46f-4df0-82c5-f39b4af6f1d2");
    public static readonly Guid HomeCategoryId = Guid.Parse("6ac41c49-72f2-4ee2-a9ac-4f612869321b");

    public static readonly Guid HeadphonesProductId = Guid.Parse("ec5ba7d8-03a6-4a15-a97c-41f7f7cc8d79");
    public static readonly Guid LampProductId = Guid.Parse("9fdb4abb-6d98-4f1f-a3ac-9034b40f6de2");

    public static readonly Guid HeadphonesBlackVariantId = Guid.Parse("cc589f42-5ff4-40e8-ad89-c4f80d60af60");
    public static readonly Guid HeadphonesWhiteVariantId = Guid.Parse("6cce416a-16b0-4dd3-8852-7f69f0f304f4");
    public static readonly Guid LampWarmVariantId = Guid.Parse("d49631cf-bd4f-418f-b425-d85fa6ea2a7a");

    public static readonly Guid HeadphonesBlackInventoryId = Guid.Parse("4fcd4fbc-4c8b-49ca-8d66-77dcac0a8475");
    public static readonly Guid HeadphonesWhiteInventoryId = Guid.Parse("d4f8c27d-5e6c-45f5-bd66-c7379a8b0452");
    public static readonly Guid LampWarmInventoryId = Guid.Parse("f86fda30-5ae7-40de-a979-cab72a24353f");

    public static readonly Guid AdminUserId = Guid.Parse("af2fbf41-5fb8-4840-a8d0-a869b9159ff9");
}
