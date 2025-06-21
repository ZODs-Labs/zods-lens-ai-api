namespace ZODs.Api.Repository.QueryParams.Interfaces
{
    public interface IPaginationQuery
    {
        int Page { get; set; }
        int PageSize { get; set; }
        string? SortBy { get; set; }
        string? SortDirection { get; set; }
        string? SearchTerm { get; set; }
    }
}
