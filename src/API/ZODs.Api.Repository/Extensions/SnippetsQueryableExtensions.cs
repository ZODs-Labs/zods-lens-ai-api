using ZODs.Api.Repository.Entities;
using ZODs.Api.Repository.Extensions.Queryable;

namespace ZODs.Api.Repository.Extensions
{
    public static class SnippetsQueryableExtensions
    {
        public static IQueryable<Snippet> WithSearchTerm(this IQueryable<Snippet> queryable, string? searchTerm)
        {
           return queryable.ContainsSearchTerm(searchTerm, nameof(Snippet.Name), nameof(Snippet.Description));
        }
    }
}