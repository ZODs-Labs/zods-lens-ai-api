using ZODs.Api.Repository.QueryParams.Interfaces;

namespace ZODs.Api.Repository.QueryParams
{
    /// <summary>
    /// Pagination query parameters.
    /// </summary>
    public class PaginationQueryParams : IPaginationQuery
    {
        public int Page { get; set; } = 0;

        public int PageSize { get; set; } = 10;

        public string? SortBy { get ; set; }

        public string? SortDirection { get; set; } = SortDirections.Ascending;

        public string? SearchTerm { get; set; } = string.Empty;

        public void Deconstruct(out int page, out int pageSize, out string? sortBy, out string? sortDirection)
        {
            page = this.Page;
            pageSize = this.PageSize;
            sortBy = this.SortBy;
            sortDirection = this.SortDirection;
        }
    }
}
