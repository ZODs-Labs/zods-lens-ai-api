using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore;
using ZODs.Api.Repository.Entities;
using System.Linq.Expressions;
using ZODs.Api.Repository.Entities.Interfaces;
using ZODs.Api.Repository.Interfaces;
using ZODs.Api.Repository.Common;
using ZODs.Api.Repository.QueryParams;
using ZODs.Api.Repository.Extensions.Queryable;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace ZODs.Api.Repository
{
    public abstract class Repository<TEntity, TContext> : IRepository<TEntity>
        where TEntity : BaseEntity
        where TContext : IdentityDbContext<
                            User,
                            Role,
                            Guid,
                            IdentityUserClaim<Guid>,
                            IdentityUserRole<Guid>,
                            IdentityUserLogin<Guid>,
                            IdentityRoleClaim<Guid>,
                            IdentityUserToken<Guid>>
    {
        private readonly DbSet<TEntity> entities;

        public Repository(TContext context)
        {
            this.Context = context ?? throw new ArgumentException(null, nameof(context));
            this.entities = context.Set<TEntity>();
        }

        protected TContext Context { get; }

        public virtual async Task<TEntity> Insert(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            await this.entities.AddAsync(entity, cancellationToken);
            return entity;
        }

        public virtual Task Update(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            this.Context.Update(entity);

            return Task.FromResult(0);
        }

        public async Task<int> ExecuteUpdateAsync(
              Guid id,
              Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls,
              CancellationToken cancellationToken = default)
        {
            return await Context.Set<TEntity>()
                                .Where(x => x.Id == id)
                                .ExecuteUpdateAsync(setPropertyCalls, cancellationToken);
        }

        public async Task<int> ExecuteUpdateManyAsync(
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls,
            CancellationToken cancellationToken = default)
        {
            return await Context.Set<TEntity>()
                                .Where(predicate)
                                .ExecuteUpdateAsync(setPropertyCalls, cancellationToken);
        }

        public virtual void Delete(Expression<Func<TEntity, bool>> predicate)
        {
            ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

            this.Context.Set<TEntity>().Where(predicate).ExecuteDelete();
        }

        public virtual async Task SoftDeleteAsync(Guid id, string deletedById, bool ignoreQueryFilters = false, CancellationToken cancellationToken = default)
        {
            if (!typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
            {
                throw new Exception($"Type {typeof(TEntity).Name} does not support soft delete, because it does not implement interface {nameof(ISoftDelete)}");
            }

            var query = this.Context.Set<TEntity>().AsQueryable();

            if (ignoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }

            var entity = await query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken: cancellationToken);

            if (entity != null && entity is ISoftDelete softDeleteEntity)
            {
                softDeleteEntity.IsDeleted = true;
                softDeleteEntity.DeletedAt = DateTime.UtcNow;
                softDeleteEntity.DeletedById = deletedById;

                await this.Update((TEntity)softDeleteEntity, cancellationToken);
            }
        }

        public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, bool disableTracking = true, bool ignoreQueryFilters = false, CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> query = this.entities;

            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (ignoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }

            return query.AnyAsync(predicate, cancellationToken);
        }

        public Task<List<TResult>> GetAllAsync<TResult>(
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include,
            Expression<Func<TEntity, TResult>>? selector,
            bool disableTracking = true,
            bool ignoreQueryFilters = false,
            CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> query = this.entities;

            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (include != null)
            {
                query = include(query);
            }

            if (ignoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }

            return query.Select(selector!).ToListAsync(cancellationToken);
        }

        public virtual Task<TEntity?> GetByIdAsync(
            Guid id,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? include,
            bool disableTracking = true,
            bool ignoreQueryFilters = false,
            CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id));
            }

            IQueryable<TEntity> query = this.entities;

            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (include != null)
            {
                query = include(query);
            }

            if (ignoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }

            return query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public Task<List<TResult>> FindByConditionProjectedAsync<TResult>(
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<TEntity, TResult>>? selector,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include,
            bool disableTracking = true,
            bool ignoreQueryFilters = false,
            CancellationToken cancellationToken = default)
        {
            if (predicate == null)
            {
                throw new ArgumentException(null, nameof(predicate));
            }

            IQueryable<TEntity> query = this.entities;

            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (include != null)
            {
                query = include(query);
            }

            if (ignoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }

            return query.Where(predicate).Select(selector!).ToListAsync(cancellationToken);
        }

        public Task<PagedEntities<TEntity>> FindByConditionAsync(
            Expression<Func<TEntity, bool>> predicate,
            PaginationQueryParams paginationQueryParams,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include,
            bool disableTracking = true,
            bool ignoreQueryFilters = false,
            CancellationToken cancellationToken = default)
        {
            if (predicate == null)
            {
                throw new ArgumentException(null, nameof(predicate));
            }

            IQueryable<TEntity> query = this.entities;

            query = query.Where(predicate);

            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (include != null)
            {
                query = include(query);
            }

            if (ignoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }

            return query.PageBy(paginationQueryParams, cancellationToken);
        }

        public Task<TEntity?> FirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include,
            bool disableTracking = true,
            bool ignoreQueryFilters = false,
            CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> query = this.entities;

            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (include != null)
            {
                query = include(query);
            }

            if (ignoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }

            return query.FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public async Task<TResult?> FirstOrDefaultAsync<TResult>(
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<TEntity, TResult>> selector,
            bool disableTracking = true,
            bool ignoreQueryFilters = false,
            CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> query = this.entities;

            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (ignoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }

            return await query.Where(predicate).Select(selector).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<TProjectedType?> FirstOrDefaultAsync<TProjectedType>(
            Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IQueryable<TProjectedType>> transformQuery,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include,
            bool disableTracking = true,
            bool ignoreQueryFilters = false,
            CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> query = this.entities;

            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (include != null)
            {
                query = include(query);
            }

            if (ignoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }

            return await transformQuery(query).FirstOrDefaultAsync(cancellationToken);
        }
    }
}