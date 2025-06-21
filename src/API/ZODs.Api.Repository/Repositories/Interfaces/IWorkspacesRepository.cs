using ZODs.Api.Repository.Common;
using ZODs.Api.Repository.Dtos;
using ZODs.Api.Repository.Entities;
using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Repository.Interfaces;
using ZODs.Api.Repository.QueryParams;

namespace ZODs.Api.Repository;

public interface IWorkspacesRepository : IRepository<Workspace>
{
    /// <summary>
    /// Get workspace members.
    /// </summary>
    /// <param name="workspaceId">Workspace id.</param>
    /// <param name="userId">Owner user id.</param>
    /// <param name="query">Query.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
    /// <returns>Paged collection of workspace members.</returns>
    Task<PagedEntities<WorkspaceMemberDto>> GetPagedWorkspaceMembersAsync(
        Guid workspaceId,
        Guid userId,
        GetPagedWorkspaceMembersQuery query,
        CancellationToken cancellationToken);

    /// <summary>
    /// Get user workspaces.
    /// </summary>
    /// <param name="userId">Owner user id.</param>
    /// <param name="query">Query.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
    /// <returns></returns>
    Task<ICollection<UserWorkspaceDto>> GetUserWorkspaces(
        Guid userId,
        GetUserWorkspacesQuery query,
        CancellationToken cancellationToken);

    /// <summary>
    /// Check if user is workspace owner.
    /// </summary>
    /// <param name="userId">Owner user id.</param>
    /// <param name="workspaceId">Workspace id.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
    /// <returns></returns>
    Task<bool> IsUserWorkspaceOwnerAsync(Guid userId, Guid workspaceId, CancellationToken cancellationToken);

    /// <summary>
    /// Check if user is workspace member.
    /// </summary>
    /// <param name="userId">User id.</param>
    /// <param name="workspaceId">Workspace id.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
    /// <returns>True if user is workspace member, otherwise false.</returns>
    Task<bool> IsUserWorkspaceMemberAsync(Guid userId, Guid workspaceId, CancellationToken cancellationToken);

    /// <summary>
    /// Checks if workspace role is lower than user workspace role.
    /// </summary>
    /// <param name="workspaceId">Workspace id.</param>
    /// <param name="userId">User id.</param>
    /// <param name="roleIndex">Role index.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
    /// <returns>True if workspace role is lower than user workspace role, otherwise false.</returns>
    Task<bool> IsWorkspaceRoleLowerThanUserWorkspaceRole(
        Guid workspaceId,
        Guid userId,
        WorkspaceMemberRoleIndex roleIndex,
        CancellationToken cancellationToken);

    /// <summary>
    /// Get role id by index.
    /// </summary>
    /// <param name="roleIndex">Role index.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
    /// <returns></returns>
    Task<Guid> GetRoleIdByIndexAsync(WorkspaceMemberRoleIndex roleIndex, CancellationToken cancellationToken);

    /// <summary>
    /// Add member with role to workspace.
    /// </summary>
    /// <param name="workspaceMember">Entity.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
    /// <returns></returns>
    Task AddMemberWithRoleToWorkspaceAsync(
        WorkspaceMember workspaceMember,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets user workspaces dropdown with specified user role index specifies which workspaces to get.
    /// </summary>
    /// <param name="userId">User id.</param>
    /// <param name="userRoleIndex">Role index to get workspaces where user has this role.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
    /// <returns>True if workspace role is lower than user workspace role, otherwise false.</returns>
    /// <returns></returns>
    Task<ICollection<WorkspaceDropdownDto>> GetUserWorkspacesDropdownAsync(
        Guid userId,
        WorkspaceMemberRoleIndex userRoleIndex,
        CancellationToken cancellationToken);

    /// <summary>
    /// Removes workspace member.
    /// </summary>
    /// <param name="workspaceMemberId">Workspace member id.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
    /// <returns></returns>
    Task RemoveWorkspaceMemberAsync(Guid workspaceMemberId, CancellationToken cancellationToken);

    Task<int> GetUserCreatedWorkspacesCount(Guid userId, CancellationToken cancellationToken = default);

    Task<IDictionary<Guid, int>> CountSnippetsByWorkspacesForUser(Guid userId, CancellationToken cancellationToken = default);

    Task<IDictionary<Guid, int>> CountInvitesByWorksapcesForUser(Guid userId, CancellationToken cancellationToken = default);
    Task DeleteAllUserOwnedWorkspacesAsync(Guid userId, CancellationToken cancellationToken);
    Task RemoveMemberFromAllWorkspacesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> UserScopedWorkspaceWithSameNameExistsAsync(string name, Guid userId, Guid? ignoreWorkspaceId, CancellationToken cancellationToken = default);
    Task<ICollection<Guid>> GetAllExclusiveMembersOfOwnerWorkspacesAsync(Guid userOwnerId, CancellationToken cancellationToken = default);
    Task SetUserWorkspacesActiveStatusAsync(
        Guid userId,
        bool isActive,
        CancellationToken cancellationToken);
    Task<Dictionary<Guid, string>> GetWorkspacesNameByIdsAsync(ICollection<Guid> workspaceIds, CancellationToken cancellationToken);
    Task IncrementWorksaceSnippetsVersionAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<int> GetAllUserWorkspacesSnippetVersionSumAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ICollection<UserWorkspaceDto>> GetUserWorkspacesForWidgetAsync(GetUserWorkspacesQuery query, Guid userId, CancellationToken cancellationToken);
}
