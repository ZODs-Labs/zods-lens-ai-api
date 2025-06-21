using ZODs.Api.Repository.Common;
using ZODs.Api.Repository.Entities;
using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Repository.Extensions.Queryable;
using ZODs.Api.Repository.QueryParams;
using Microsoft.EntityFrameworkCore;

namespace ZODs.Api.Repository
{
    public sealed class SnippetTriggerPrefixesRepository : Repository<SnippetTriggerPrefix, ZodsContext>, ISnippetTriggerPrefixesRepository
    {
        public SnippetTriggerPrefixesRepository(ZodsContext context)
            : base(context)
        {
        }

        public async Task<PagedEntities<SnippetTriggerPrefix>> GetPagedUserSnippetTriggerPrefixes(
            GetUserSnippetTriggerPrefixesQuery query,
            Guid userId,
            CancellationToken cancellationToken)
        {
            var queryable = Context.SnippetTriggerPrefixes
                                   .AsNoTracking()
                                   .Where(x => x.UserId == userId)
                                   .ContainsSearchTerm(
                                         query.SearchTerm,
                                         nameof(SnippetTriggerPrefix.Prefix));

            var pagedEntities = await queryable.PageBy(query, cancellationToken);

            return pagedEntities;
        }

        public async Task<PagedEntities<SnippetTriggerPrefix>> GetPagedWorkspaceSnippetTriggerPrefixes(
            GetWorkspaceSnippetTriggerPrefixesQuery query,
            Guid workspaceId,
            CancellationToken cancellationToken)
        {
            var queryable = Context.SnippetTriggerPrefixes
                                   .AsNoTracking()
                                   .Where(x => x.WorkspaceId == workspaceId)
                                   .ContainsSearchTerm(
                                         query.SearchTerm,
                                         nameof(SnippetTriggerPrefix.Prefix));

            var pagedEntities = await queryable.PageBy(query, cancellationToken);

            return pagedEntities;
        }

        public async Task<int> CountPersonalSnippetPrefixesAsync(
            Guid userId,
            CancellationToken cancellationToken)
        {
            var prefixesCount = await Context.SnippetTriggerPrefixes
                .Where(x => x.UserId == userId)
                .CountAsync(cancellationToken);

            return prefixesCount;
        }

        public async Task<Dictionary<Guid, int>> CountSnippetPrefixesByWorkspaceAsync(
            Guid userId,
            CancellationToken cancellationToken)
        {
            var prefixesCount = await Context.Workspaces.Where(
                x => x.Members.Any(m => m.UserId == userId &&
                                        m.Roles.Any(r => r.Role.Index == WorkspaceMemberRoleIndex.Owner)))
                 .Include(x => x.TriggerPrefixes)
                 .ToDictionaryAsync(x => x.Id, x => x.TriggerPrefixes.Count, cancellationToken);

            return prefixesCount;
        }

        public async Task DeleteAllUserSnippetTriggerPrefixesAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            await Context.SnippetTriggerPrefixes
                         .Where(x => x.UserId == userId)
                         .ExecuteDeleteAsync(cancellationToken);
        }
    }
}