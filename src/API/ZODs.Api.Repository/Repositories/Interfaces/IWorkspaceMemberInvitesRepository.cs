using ZODs.Api.Repository.Common;
using ZODs.Api.Repository.Dtos;
using ZODs.Api.Repository.Entities;
using ZODs.Api.Repository.Interfaces;
using ZODs.Api.Repository.QueryParams;

namespace ZODs.Api.Repository
{
    public interface IWorkspaceMemberInvitesRepository: IRepository<WorkspaceMemberInvite>
    {
        /// <summary>
        /// Get paged workspace invited members.
        /// </summary>
        /// <param name="workspaceId">Workspace id.</param>
        /// <param name="executingUserId">Owner user id.</param>
        /// <param name="query">Query.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
        /// <returns></returns>
        Task<PagedEntities<WorkspaceInviteMemberDto>> GetWorkspaceInvitedMembersAsync(
            Guid workspaceId,
            Guid executingUserId,
            GetPagedWorkspaceInvitedMembersQuery query,
            CancellationToken cancellationToken);
    }
}