using ZODs.Api.Repository.QueryParams;
using ZODs.Api.Service.Dtos;
using ZODs.Api.Service.InputDtos.Snippet;

namespace ZODs.Api.Service;

public interface ISnippetsService
{
    /// <summary>
    /// Get user snippets overview.
    /// </summary>
    /// <param name="query">Query params.</param>
    /// <param name="userId">User id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User snippets.</returns>
    Task<UserSnippetsOverviewDto> GetUserSnippetsOverviewAsync(GetUserSnippetsQuery query, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets snippets shared with user.
    /// </summary>
    /// <param name="query">Query params.</param>
    /// <param name="userId">User id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    Task<UserSnippetsOverviewDto> GetSnippetsSharedWithUserAsync(GetUserSnippetsQuery query, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets user own snippets.
    /// </summary>
    /// <param name="query">Query params.</param>
    /// <param name="userId">User id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    Task<UserSnippetsOverviewDto> GetUserOwnSnippetsAsync(GetUserSnippetsQuery query, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets user snippets.
    /// </summary>
    /// <param name="userId">User id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User snippets.</returns>
    Task<UserSnippetsForIntegrationDto> GetUserSnippetsForIntegrationAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets snippet code.
    /// </summary>
    /// <param name="snippetId">Snippet id.</param>
    /// <param name="userId">User id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Snippet code.</returns>
    Task<string> GetSnippetCodeAsync(Guid snippetId, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new snippet.
    /// </summary>
    /// <param name="snippetDto">Snippet to create.</param>
    /// <param name="userId">User id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created snippet.</returns>
    Task<SnippetDto> CreateSnippetAsync(UpsertSnippetInputDto snippetDto, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Updates snippet.
    /// </summary>
    /// <param name="snippetDto">Snippet to update.</param>
    /// <param name="userId">User id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated snippet.</returns>
    Task<SnippetDto> UpdateSnippetAsync(UpsertSnippetInputDto snippetDto, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets user snippets version.
    /// </summary>
    /// <param name="userId">User id/</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    Task<long> GetUserSnippetsVersionAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets user snippet by id.
    /// </summary>
    /// <param name="snippetId">Snippet id.</param>
    /// <param name="userId">User id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Found snippet.</returns>
    Task<SnippetDto> GetUserSnippetByIdAsync(
        Guid snippetId,
        Guid userId,
        CancellationToken cancellationToken);

    Task IncrementSnippetsVersionAsync(Guid userId, Guid? workspaceId, bool isWorkspaceOwned, CancellationToken cancellationToken);
}