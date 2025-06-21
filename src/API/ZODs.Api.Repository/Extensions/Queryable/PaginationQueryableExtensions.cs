using ZODs.Api.Common.Extensions;
using ZODs.Api.Repository.Common;
using ZODs.Api.Repository.QueryParams;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace ZODs.Api.Repository.Extensions.Queryable
{
    public static class PaginationQueryableExtensions
    {
        /// <summary>
        /// Applies pagination and sorting then transforms the paged entities to a different type.
        /// </summary>
        /// <typeparam name="T">The type of elements in the source query.</typeparam>
        /// <typeparam name="TTransformedType">The type of elements in the transformed query.</typeparam>
        /// <param name="query">An IQueryable of elements of type T.</param>
        /// <param name="transformQuery">Function to transform entities from type T to type TTransformedType.</param>
        /// <param name="paginationParams">Pagination parameters.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A PagedEntities of type TTransformedType containing paged, sorted, filtered, and transformed elements.</returns>
        /// <exception cref="ArgumentNullException">Thrown when query is null.</exception>
        public static async Task<PagedEntities<TTransformedType>> PageBy<T, TTransformedType>(
            this IQueryable<T> query,
            Func<IQueryable<T>, IQueryable<TTransformedType>> transformQuery,
            PaginationQueryParams paginationParams,
            CancellationToken cancellationToken = default)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            var (page, pageSize, sortBy, sortDirection) = paginationParams;

            var sortByColumns = sortBy.SplitIfNotEmpty().Select(x => x?.Trim()).ToArray();
            var totalSortByColumns = sortByColumns.Length;
            var isDescendingOrder = sortDirection == SortDirections.Descending;
            var sortDir = isDescendingOrder ? SortDirections.Descending : SortDirections.Ascending;

            if (totalSortByColumns > 0)
            {
                sortBy = sortByColumns[0];
                var orderedQuery = query.OrderBy($"{sortBy} {sortDir}");

                if (totalSortByColumns > 1)
                {
                    foreach (var column in sortByColumns.Skip(1).ToArray())
                    {
                        orderedQuery = orderedQuery.ThenBy($"{column} {sortDir}");
                    }
                }

                query = orderedQuery;
            }

            var transformedQuery = transformQuery(query);

            var entities = transformedQuery.Skip((page <= 0 ? 0 : page) * pageSize).Take(pageSize);
            var entitiesCount = await transformedQuery.CountAsync(cancellationToken: cancellationToken);

            return new PagedEntities<TTransformedType>
            {
                Entities = await entities.ToListAsync(cancellationToken: cancellationToken),
                TotalCount = entitiesCount,
            };
        }

        /// <summary>
        /// Applies pagination and sorting to an IQueryable of elements of type T.
        /// </summary>
        /// <typeparam name="T">The type of elements in the source query.</typeparam>
        /// <param name="query">An IQueryable of elements of type T.</param>
        /// <param name="paginationParams">Pagination parameters.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A PagedEntities of type T containing paged, sorted, and filtered elements.</returns>
        /// <exception cref="ArgumentNullException">Thrown when query is null.</exception>
        public static async Task<PagedEntities<T>> PageBy<T>(
            this IQueryable<T> query,
            PaginationQueryParams paginationParams,
            CancellationToken cancellationToken = default)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            var (page, pageSize, sortBy, sortDirection) = paginationParams;

            var sortByColumns = sortBy.SplitIfNotEmpty().Select(x => x?.Trim()).ToArray();
            var totalSortByColumns = sortByColumns.Length;
            var isDescendingOrder = sortDirection == SortDirections.Descending;
            var sortDir = isDescendingOrder ? SortDirections.Descending : SortDirections.Ascending;

            if (totalSortByColumns > 0)
            {
                sortBy = sortByColumns[0];
                var orderedQuery = query.OrderBy($"{sortBy} {sortDir}");

                if (totalSortByColumns > 1)
                {
                    foreach (var column in sortByColumns.Skip(1).ToArray())
                    {
                        orderedQuery = orderedQuery.ThenBy($"{column} {sortDir}");
                    }
                }

                query = orderedQuery;
            }

            var entities = query.Skip((page <= 0 ? 0 : page) * pageSize).Take(pageSize);
            var entitiesCount = await query.CountAsync(cancellationToken: cancellationToken);

            return new PagedEntities<T>
            {
                Entities = await entities.ToListAsync(cancellationToken: cancellationToken),
                TotalCount = entitiesCount,
            };
        }
    }
}