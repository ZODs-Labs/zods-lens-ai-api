using ZODs.Api.Repository.Dtos;
using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Repository.QueryParams;
using ZODs.Api.Service.Dtos;
using ZODs.Api.Service.InputDtos.Workspace;

namespace ZODs.Api.Service;

/// <summary>
/// Provides methods for managing workspaces.
/// </summary>
public interface IWorkspaceService
{
    /// <summary>
    /// Retrieves the workspace information by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the workspace.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing the workspace details.</returns>
    Task<WorkspaceDto> GetWorkspaceById(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Get workspace details for user
    /// </summary>
    /// <param name="id">The unique identifier of the workspace.</param>
    /// <param name="executingUserId">The unique identifier of the user requesting the workspace members.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, workspace details.</returns>
    Task<UserWorkspaceDto> GetWorkspaceDetailsAsync(Guid id, Guid executingUserId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a paginated list of workspaces for a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user requesting the workspaces.</param>
    /// <param name="query">Query parameters</param>
    /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a paginated list of workspaces.</returns>
    Task<UserWorkspacesDto> GetUserWorkspacesAsync(
        Guid userId,
        GetUserWorkspacesQuery query,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets a paginated list of members in a specific workspace.
    /// </summary>
    /// <param name="workspaceId">The unique identifier of the workspace.</param>
    /// <param name="userId">The unique identifier of the user requesting the workspace members.</param>
    /// <param name="query">Query parameters for pagination.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a paginated list of workspace members.</returns>
    Task<PagedResponse<WorkspaceMemberDto>> GetWorkspaceMembersAsync(
        Guid workspaceId,
        Guid userId,
        GetPagedWorkspaceMembersQuery query,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets a paginated list of invited members in a specific workspace.
    /// </summary>
    /// <param name="workspaceId">The unique identifier of the workspace.</param>
    /// <param name="executingUserId">The unique identifier of the user requesting the workspace members.</param>
    /// <param name="query">Query parameters for pagination.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns></returns>
    Task<PagedResponse<WorkspaceInviteMemberDto>> GetPagedWorkspaceInvitedMembersAsync(
        Guid workspaceId,
        Guid executingUserId,
        GetPagedWorkspaceInvitedMembersQuery query,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets a paginated list of snippets in a specific workspace.
    /// </summary>
    /// <param name="workspaceId">Workspace id.</param>
    /// <param name="executingUserId">Executing user id.</param>
    /// <param name="query">Query.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing a paginated list of workspace snippets.</returns>
    Task<PagedResponse<SnippetOverviewDto>> GetWorkspaceSnippetsAsync(
        Guid workspaceId,
        Guid executingUserId,
        GetWorkspaceSnippetsQuery query,
        CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new workspace.
    /// </summary>
    /// <param name="workspaceDto">The data transfer object containing the workspace details.</param>
    /// <param name="executingUserId">User that init this action.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing the created workspace details.</returns>
    Task<WorkspaceDto> CreateWorkspaceAsync(
        WorkspaceDto workspaceDto,
        Guid executingUserId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing workspace.
    /// </summary>
    /// <param name="workspaceDto">The data transfer object containing the updated workspace details.</param>
    /// <param name="userId">The unique identifier of the user requesting the update.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing the updated workspace details.</returns>
    Task<WorkspaceDto> UpdateWorkspaceAsync(
        WorkspaceDto workspaceDto,
        Guid userId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Send invitation to user to join workspace.
    /// </summary>
    /// <param name="workspaceId">Workspace to send an invitation for.</param>
    /// <param name="inputDto"><see cref="InviteWorkspaceMemberInputDto"/>.</param>
    /// <param name="executingUserId">Excecuting user id.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task InviteMemberToWorkspaceAsync(
        Guid workspaceId,
        InviteWorkspaceMemberInputDto inputDto,
        Guid executingUserId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Checks if workspace member invite is valid.
    /// </summary>
    /// <param name="workspaceInviteId">Invite id.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns></returns>
    Task<bool> IsWorkspaceMemberInviteValidAsync(
        Guid workspaceInviteId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets Workspace invite info.
    /// </summary>
    /// <param name="workspaceInviteId">Workspace invite id.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns>Workspace invite info.</returns>
    Task<WorkspaceInviteInfoDto> GetWorkspaceInviteInfoAsync(
        Guid workspaceInviteId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Accept workspace member invite.
    /// </summary>
    /// <param name="workspaceInviteId">Workspace invite id.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation.</param>
    /// <returns></returns>
    Task AcceptWorkspaceMemberInviteAsync(
        Guid workspaceInviteId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets user workspaces dropdown with specified user role index specifies which workspaces to get.
    /// </summary>
    /// <param name="userId">User id.</param>
    /// <param name="userRoleIndex">Role index to get workspaces where user has this role.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
    /// <returns>True if workspace role is lower than user workspace role, otherwise false.</returns>
    /// <returns>Collection of <see cref="WorkspaceDropdownDto"/>.</returns>
    Task<ICollection<WorkspaceDropdownDto>> GetUserWorkspacesDropdownAsync(
        Guid userId,
        WorkspaceMemberRoleIndex userRoleIndex,
        bool forSnippetCreate,
        CancellationToken cancellationToken);

    /// <summary>
    /// Removes member from workspace.
    /// </summary>
    /// <param name="workspaceMemberId">Workspace member id.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task RemoveMemberFromWorkspaceAsync(Guid workspaceMemberId, CancellationToken cancellationToken);
    Task<UserWorkspacesDto> GetUserWorkspacesForWidgetAsync(GetUserWorkspacesQuery query, Guid userId, CancellationToken cancellationToken);
}
