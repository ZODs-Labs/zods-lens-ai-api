using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ZODs.Api.Repository.Extensions.Queryable;

public static class FilteringQueryableExtensions
{
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition, Expression<Func<T, bool>> predicate)
    {
        return condition
            ? query.Where(predicate)
            : query;
    }

    public static IQueryable<T> TakeIf<T>(this IQueryable<T> query, string sortBy, bool condition, int limit, bool sortByDescending = true)
    {
        // It is necessary sort items before it
        if (!string.IsNullOrEmpty(sortBy))
        {
            query = query.OrderBy(x => $"x.{sortBy} {(sortByDescending ? SortDirections.Descending : SortDirections.Ascending)}");
        }

        return condition
            ? query.Take(limit)
            : query;
    }

    /// <summary>
    /// Filters a sequence of values based on a search term and target properties.
    /// Uses a case-insensitive search to filter the records where any of the specified properties 
    /// contain the provided search term.
    /// </summary>
    /// <typeparam name="T">The type of the elements of <paramref name="query"/>.</typeparam>
    /// <param name="query">An IQueryable<T> to filter.</param>
    /// <param name="searchTerm">The term to search for.</param>
    /// <param name="properties">One or more property names to search within.</param>
    /// <returns>
    /// An <see cref="IQueryable{T}"/> that contains elements that satisfy the condition specified
    /// by <paramref name="searchTerm"/> and <paramref name="properties"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="query"/> or <paramref name="properties"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="properties"/> does not contain any valid property names for type <typeparamref name="T"/>.
    /// </exception>
    /// <example>
    /// <code>
    /// var filteredQuery = originalQuery.ContainsSearchTerm("searchTerm", "PropertyName1", "PropertyName2");
    /// </code>
    /// </example>
    public static IQueryable<T> ContainsSearchTerm<T>(this IQueryable<T> query, string? searchTerm, params string[] properties)
    {
        // Input validation
        if (query == null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        if (properties == null || properties.Length == 0)
        {
            throw new ArgumentException("At least one property should be provided", nameof(properties));
        }

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return query;  // Return original query if searchTerm is null or whitespace
        }

        var entityType = typeof(T);

        // Build dynamic query
        var parameter = Expression.Parameter(entityType, "x");
        Expression predicate = null!;

        foreach (var propPath in properties)
        {
            var propNames = propPath.Split('.');
            var propertyType = typeof(T);
            Expression propAccess = parameter;

            foreach (var propName in propNames)
            {
                var propertyInfo = propertyType.GetProperty(propName);
                if (propertyInfo == null)
                {
                    throw new ArgumentException($"Property '{propName}' does not exist on type '{propertyType.Name}'.", nameof(properties));
                }

                propAccess = Expression.PropertyOrField(propAccess, propName);
                propertyType = propertyInfo.PropertyType;
            }

            var likeMethod = typeof(NpgsqlDbFunctionsExtensions).GetMethod(nameof(NpgsqlDbFunctionsExtensions.ILike),
                new[] { typeof(DbFunctions), typeof(string), typeof(string) });
            if (likeMethod == null)
            {
                throw new InvalidOperationException($"Method '{nameof(NpgsqlDbFunctionsExtensions.ILike)}' does not exist on type '{typeof(NpgsqlDbFunctionsExtensions).Name}'.");
            }

            var dbFunctions = Expression.Constant(EF.Functions);
            var searchTermExpr = Expression.Constant($"%{searchTerm}%");

            var singlePropPredicate = Expression.Call(null, likeMethod, dbFunctions, propAccess, searchTermExpr);
            predicate = predicate == null ? singlePropPredicate : Expression.OrElse(predicate, singlePropPredicate);
        }

        predicate ??= Expression.Constant(true);

        // Finalize and execute dynamic query
        var lambda = Expression.Lambda<Func<T, bool>>(predicate, parameter);
        return query.Where(lambda);
    }
}
