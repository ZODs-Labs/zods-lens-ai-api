using Microsoft.EntityFrameworkCore;

namespace ZODs.Api.Repository.Interfaces
{
    /// <summary>
    /// Defines the interface(s) for generic unit of work.
    /// </summary>
    /// <typeparam name="TContext">Database context.</typeparam>
    public interface IUnitOfWork<TContext>
        where TContext : DbContext
    {
        /// <summary>
        /// Gets the specified repository for the <typeparamref name="TIRepository"/>.
        /// </summary>
        /// <typeparam name="TIRepository">The type of the entity.</typeparam>
        /// <returns>An instance of type inherited from <typeparamref name="TIRepository"/> interface.</returns>
        TIRepository GetRepository<TIRepository>()
            where TIRepository : class;

        /// <summary>
        /// Persists changes made on database context to database.
        /// </summary>
        void Commit();

        /// <summary>
        /// Rollbacks any changes made on database context.
        /// </summary>
        void Rollback();

        /// <summary>
        /// Persists changes made on database context to database asynchronus.
        /// </summary>
        /// <returns>Task.</returns>
        Task CommitAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Rollbacks any changes made on database context asynchronus.
        /// </summary>
        /// <returns>ValueTask.</returns>
        ValueTask RollbackAsync();
    }
}
