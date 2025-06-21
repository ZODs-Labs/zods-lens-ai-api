using ZODs.Api.Repository.Common;
using ZODs.Api.Repository.Entities;
using ZODs.Api.Repository.Interfaces;
using ZODs.Api.Repository.QueryParams;

namespace ZODs.Api.Repository
{
    public interface ISnippetTriggerPrefixesRepository : IRepository<SnippetTriggerPrefix>
    {
        Task<int> CountPersonalSnippetPrefixesAsync(Guid userId, CancellationToken cancellationToken);
        Task<Dictionary<Guid, int>> CountSnippetPrefixesByWorkspaceAsync(Guid userId, CancellationToken cancellationToken);
        Task DeleteAllUserSnippetTriggerPrefixesAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<PagedEntities<SnippetTriggerPrefix>> GetPagedUserSnippetTriggerPrefixes(GetUserSnippetTriggerPrefixesQuery query, Guid userId, CancellationToken cancellationToken);
        Task<PagedEntities<SnippetTriggerPrefix>> GetPagedWorkspaceSnippetTriggerPrefixes(GetWorkspaceSnippetTriggerPrefixesQuery query, Guid workspaceId, CancellationToken cancellationToken);
    }
}