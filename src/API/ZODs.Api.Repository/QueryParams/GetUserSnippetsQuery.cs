using ZODs.Api.Repository.Entities;
using ZODs.Api.Repository.QueryParams.Interfaces;
using ZODs.Common.Attributes;

namespace ZODs.Api.Repository.QueryParams;

public sealed class GetUserSnippetsQuery : PaginationQueryParams, ISnippetsPaginatedQuery
{
    public GetUserSnippetsQuery()
    {
    }
}