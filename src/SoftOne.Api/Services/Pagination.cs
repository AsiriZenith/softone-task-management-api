namespace SoftOne.Api.Services;

internal static class Pagination
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 5;
    public const int MaxPageSize = 50;

    public static (int Page, int PageSize) Normalize(int? page, int? pageSize)
    {
        var normalizedPage = page is null or < 1 ? DefaultPage : page.Value;
        var normalizedPageSize = pageSize is null or < 1
            ? DefaultPageSize
            : Math.Min(pageSize.Value, MaxPageSize);

        return (normalizedPage, normalizedPageSize);
    }

    public static int CalculateTotalPages(int totalCount, int pageSize) =>
        pageSize <= 0 || totalCount <= 0
            ? 0
            : (int)Math.Ceiling(totalCount / (double)pageSize);
}
