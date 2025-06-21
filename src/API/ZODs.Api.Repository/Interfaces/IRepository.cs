using Microsoft.EntityFrameworkCore.Query;
using ZODs.Api.Repository.Common;
using System.Linq.Expressions;
using ZODs.Api.Repository.QueryParams;

namespace ZODs.Api.Repository.Interfaces
{
    /// <summary>
    /// Generic repository for entity CRUD operations.
    /// </summary>
    /// <typeparam name="TEntity">Type of entity.</typeparam>
    public interface IRepository<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Returns all entities.
        /// </summary>
        /// <param name="include">A function to include navigation properties.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <param name="disableTracking"><c>true</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
        /// <param name="ignoreQueryFilters">Ignore query filters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of TEntity.</returns>
        Task<List<TResult>> GetAllAsync<TResult>(
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
            Expression<Func<TEntity, TResult>>? selector = null,
            bool disableTracking = true,
            bool ignoreQueryFilters = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns entity by Id.
        /// </summary>
        /// <param name="id">Identifier.</param>
        /// <param name="include">A function to include navigation properties.</param>
        /// <param name="disableTracking"><c>true</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
        /// <param name="ignoreQueryFilters">Ignore query filters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>TEntity.</returns>
        Task<TEntity?> GetByIdAsync(
            Guid id,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
            bool disableTracking = true,
            bool ignoreQueryFilters = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Inserts new entity to database.
        /// No effect until context.SaveChanges() is called.
        /// </summary>
        /// <param name="entity">TEntity.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>New TEntity.</returns>
        Task<TEntity> Insert(
            TEntity entity,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Starts tracking changes on entity in Database Context.
        /// No effect until context.SaveChanges() is called.
        /// </summary>
        /// <param name="entity">TEntity.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task Update(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        void Delete(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Soft deletes entity with specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">Id.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <param name="deletedById">Id of member that deletes entity..</param>
        /// <param name="ignoreQueryFilters">Ignore query filters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task SoftDeleteAsync(Guid id, string deletedById, bool ignoreQueryFilters = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if entity exists based on predicate.
        /// </summary>
        /// <param name="predicate">Predicate.</param>
        /// <param name="disableTracking"><c>true</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
        /// <param name="ignoreQueryFilters">Ignore query filters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>bool.</returns>
        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, bool disableTracking = true, bool ignoreQueryFilters = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns entities mached by predicate.
        /// </summary>
        /// <param name="predicate">Predicate.</param>
        /// <param name="include">A function to include navigation properties.</param>
        /// <param name="disableTracking"><c>true</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
        /// <param name="ignoreQueryFilters">Ignore query filters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List{TEntity}.</returns>
        Task<List<TResult>> FindByConditionProjectedAsync<TResult>(
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<TEntity, TResult>>? selector,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? includee = null,
            bool disableTracking = true,
            bool ignoreQueryFilters = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns single entity or null matched by predicate.
        /// </summary>
        /// <param name="predicate">Predicate.</param>
        /// <param name="include">A function to include navigation properties.</param>
        /// <param name="disableTracking"><c>true</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
        /// <param name="ignoreQueryFilters">Ignore query filters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>TEntity.</returns>
        Task<TEntity?> FirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
            bool disableTracking = true,
            bool ignoreQueryFilters = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns single <typeparamref name="TResult"/> or null matched by predicate.
        /// </summary>
        /// <param name="predicate">Predicate.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <param name="disableTracking"><c>true</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
        /// <param name="ignoreQueryFilters">Ignore query filters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <typeparam name="TResult">Type of result.</typeparam>
        /// <returns>TEntity.</returns>
        Task<TResult?> FirstOrDefaultAsync<TResult>(
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<TEntity, TResult>> selector,
            bool disableTracking = true,
            bool ignoreQueryFilters = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns single <typeparamref name="TProjectedType"/> or null matched by predicate.
        /// </summary>
        /// <typeparam name="TProjectedType">Projected type.</typeparam>
        /// <param name="predicate">Predicate.</param>
        /// <param name="transformQuery">Transform query function.</param>
        /// <param name="include">A function to include navigation properties.</param>
        /// <param name="disableTracking"><c>true</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
        /// <param name="ignoreQueryFilters">Ignore query filters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Found instance of <typeparamref name="TEntity"/>.</returns>
        Task<TProjectedType?> FirstOrDefaultAsync<TProjectedType>(
            Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IQueryable<TProjectedType>> transformQuery,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
            bool disableTracking = true,
            bool ignoreQueryFilters = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns paged entities matched by predicate.
        /// </summary>
        /// <param name="predicate">Results filter predicate.</param>
        /// <param name="sortBy">Propery to order by.</param>
        /// <param name="page">Page to fetch.</param>
        /// <param name="pageSize">Page size to fetch.</param>
        /// <param name="sortDirection">Should order by asc or desc.</param>
        /// <param name="include">A function to include navigation properties.</param>
        /// <param name="disableTracking"><c>true</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
        /// <param name="ignoreQueryFilters">Ignore query filters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>TEntity list.</returns>
        Task<PagedEntities<TEntity>> FindByConditionAsync(
            Expression<Func<TEntity, bool>> predicate,
            PaginationQueryParams paginationQueryParams,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
            bool disableTracking = true,
            bool ignoreQueryFilters = false,
            CancellationToken cancellationToken = default);
        Task<int> ExecuteUpdateAsync(Guid id, Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls, CancellationToken cancellationToken = default);
        Task<int> ExecuteUpdateManyAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls, CancellationToken cancellationToken = default);
    }
}
