using ZODs.Api.Repository.Common;
using ZODs.Api.Repository.Dtos;
using ZODs.Api.Repository.Entities;
using ZODs.Api.Repository.Interfaces;
using ZODs.Api.Repository.QueryParams;

namespace ZODs.Api.Repository;

public interface ISnippetsRepository : IRepository<Snippet>
{
    /// <summary>
    /// Gets user snippet by id.
    /// </summary>
    /// <param name="snippetId">Snippet id.</param>
    /// <param name="executingUserId">Id of user that initiates the flow.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Snippet.</returns>
    Task<Snippet?> GetUserSnippetByIdAsync(Guid snippetId, Guid executingUserId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets user snippets overview;
    /// </summary>
    /// <param name="userId">User id.</param>
    /// <param name="query">Query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    Task<PagedEntities<SnippetOverviewDto>> GetUserSnippetsOverviewAsync(Guid userId, GetUserSnippetsQuery query, CancellationToken cancellationToken);

    /// <summary>
    /// Gets user own snippets.
    /// </summary>
    /// <param name="userId">User id.</param>
    /// <param name="query">Query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    Task<PagedEntities<SnippetOverviewDto>> GetUserOwnSnippets(
        Guid userId,
        GetUserSnippetsQuery query,
        CancellationToken cancellationToken);

    /// <summary>
    /// Get snippets shared with workspaces where user is a member.
    /// </summary>
    /// <param name="userId">User id.</param>
    /// <param name="query">Query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    Task<PagedEntities<SnippetOverviewDto>> GetSnippetsSharedWithUser(
        Guid userId,
        GetUserSnippetsQuery query,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets user snippets for integration.
    /// </summary>
    /// <param name="userId">User id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    Task<ICollection<SnippetForIntegrationDto>> GetUserSnippetsForIntegrationAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets workspace snippets.
    /// </summary>
    /// <param name="workspaceId">Workspace id.</param>
    /// <param name="executingUserId">Executing user id.</param>
    /// <param name="query">Query params.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Workspace snippets.</returns>
    Task<PagedEntities<SnippetOverviewDto>> GetWorkspaceSnippetsOverviewAsync(
        Guid workspaceId,
        Guid executingUserId,
        GetWorkspaceSnippetsQuery query,
        CancellationToken cancellationToken);

    /// <summary>
    /// Creates snippet.
    /// </summary>
    /// <param name="snippet">Snippet entity to create.</param>
    /// <param name="ownerWorkspaceId">Option owner workspace id if snippet is workspace owned.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    Task<Snippet> CreateSnippetAsync(Snippet snippet, Guid? ownerWorkspaceId, CancellationToken cancellationToken);

    /// <summary>
    /// Updates snippet.
    /// </summary>
    /// <param name="snippet">Snippet entity to update.</param>
    /// <param name="ownerWorkspaceId">Option owner workspace id if snippet is workspace owned.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated snipped entity.</returns>
    Task<Snippet> UpdateSnippetAsync(Snippet snippet, Guid? ownerWorkspaceId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets snippet code.
    /// </summary>
    /// <param name="snippetId">Snippet id.</param>
    /// <param name="userId">User id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Snippet code.</returns>
    Task<string> GetSnippetCodeAsync(Guid snippetId, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Increments user snippets version.
    /// </summary>
    /// <param name="userId">user id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    Task IncrementUserSnippetsVersion(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets user snippets version.
    /// </summary>
    /// <param name="userId">User id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    Task<long> GetUserSnippetsVersionAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Checks if user snippet exists.
    /// </summary>
    /// <param name="snippetId">Snippet id to check for.</param>
    /// <param name="userId">User id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    Task<bool> UserSnippetExists(Guid snippetId, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Checks if user has snippet write permission.
    /// </summary>
    /// <param name="snippetId">Snippet id to check for.</param>
    /// <param name="userId">User id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    Task<bool> HasUserSnippetWritePermission(Guid snippetId, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Checks if duplicate snippet exists.
    /// </summary>
    /// <param name="name">Name of a snippet.</param>
    /// <param name="trigger">Trigger of a snippet.</param>
    /// <param name="snippetId">Optional existing snippet id.</param>
    /// <param name="workspaceId">Workspace id.</param>
    /// <param name="userId">User id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    Task<bool> DuplicateSnippetExists(
        string name,
        string trigger,
        Guid? snippetId,
        Guid? workspaceId,
        Guid userId,
        bool isWorkspaceOwned,
        CancellationToken cancellationToken);

    Task<int> GetUserOwnSnippetsCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task DeleteAllUserSnippetsAsync(Guid userId, CancellationToken cancellationToken);
}