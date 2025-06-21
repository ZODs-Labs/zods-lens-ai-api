using ZODs.Api.Repository.Common;
using ZODs.Api.Repository.Dtos;
using ZODs.Api.Repository.Entities;
using ZODs.Api.Repository.Extensions;
using ZODs.Api.Repository.Extensions.Queryable;
using ZODs.Api.Repository.QueryParams;
using ZODs.Common.Extensions;
using Microsoft.EntityFrameworkCore;

namespace ZODs.Api.Repository
{
    public sealed class WorkspaceMemberInvitesRepository : Repository<WorkspaceMemberInvite, ZodsContext>, IWorkspaceMemberInvitesRepository
    {
        public WorkspaceMemberInvitesRepository(ZodsContext context)
        : base(context)
        {
        }

        public async Task<PagedEntities<WorkspaceInviteMemberDto>> GetWorkspaceInvitedMembersAsync(
            Guid workspaceId,
            Guid executingUserId,
            GetPagedWorkspaceInvitedMembersQuery query,
            CancellationToken cancellationToken)
        {
            var queryable = this.Context.WorkspaceMemberInvites
                .Include(x => x.InvitedByUser)
                .Where(x => x.WorkspaceId == workspaceId)
                .BuildProtectedWorkspaceMemberInvitesOwnerOnlyAccessQuery(executingUserId)
                .ContainsSearchTerm(query.SearchTerm,
                                    nameof(WorkspaceMemberInvite.Email),
                                    $"{nameof(WorkspaceMemberInvite.InvitedByUser)}.{nameof(WorkspaceMemberInvite.InvitedByUser.FirstName)}",
                                    $"{nameof(WorkspaceMemberInvite.InvitedByUser)}.{nameof(WorkspaceMemberInvite.InvitedByUser.LastName)}");

            Func<IQueryable<WorkspaceMemberInvite>, IQueryable<WorkspaceInviteMemberDto>> transformMapper =
               wmQueryable => wmQueryable.Select(x => new WorkspaceInviteMemberDto
               {
                   Email = x.Email,
                   RoleIndex = x.RoleIndex,
                   ExpiresAt = x.ExpiresAt,
                   IsAccepted = x.AcceptedAt != null,
                   InvitedBy = $"{x.InvitedByUser!.FirstName} {x.InvitedByUser.LastName}",
                   InvitedAt = x.CreatedAt,
               });

            var pagedEntities = await queryable.PageBy(
                transformQuery: transformMapper,
                query,
                cancellationToken).NoSync();

            return pagedEntities;

        }
    }
}