namespace FulSpectrum.Application.Catalog.Queries;

public sealed class ProductListQuery
{
    public string? Search { get; init; }
    public Guid? CategoryId { get; init; }
    public bool? IsPublished { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public string SortBy { get; init; } = "createdAt";
    public string SortDirection { get; init; } = "desc";
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
