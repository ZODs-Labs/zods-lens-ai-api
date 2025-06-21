using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Repository.Entities;

namespace ZODs.Api.Repository.Extensions
{
    public static class WorkspaceQueryableExtensions
    {
        public static IQueryable<WorkspaceMemberInvite> BuildProtectedWorkspaceMemberInvitesOwnerOnlyAccessQuery(
             this IQueryable<WorkspaceMemberInvite> query,
             Guid executingUserId)
        {
            if (executingUserId == default)
            {
                throw new ArgumentException($"Executing user id has value {executingUserId}");
            }

            return query.Where(x => x.Workspace.Members.Any(m => m.UserId == executingUserId &&
                                                                 m.Roles.Any(r => r.Role.Index == WorkspaceMemberRoleIndex.Owner)));
        }

        public static IQueryable<Workspace> BuildProtectedOwnerWorkspacesQuery(
            this IQueryable<Workspace> query,
            Guid userId)
        {
            if(userId == default)
            {
                throw new ArgumentException($"User id has value {userId}");
            }

            return query.Where(x => x.Members.Any(m => m.UserId == userId &&
                                                       m.Roles.Any(r => r.Role.Index == WorkspaceMemberRoleIndex.Owner)));
        }

        public static IQueryable<Workspace> BuildProtectedWorkspaceAccessQuery(
           this IQueryable<Workspace> query,
           Guid executingUserId)
        {
            if (executingUserId == default)
            {
                throw new ArgumentException($"Executing user id has value {executingUserId}");
            }

            return query.Where(x => x.Members.Any(m => m.UserId == executingUserId));
        }

        public static IQueryable<WorkspaceMember> BuildProtectedWorkspaceAccessQuery(
            this IQueryable<WorkspaceMember> query,
            Guid executingUserId)
        {
            if (executingUserId == default)
            {
                throw new ArgumentException($"Executing user id has value {executingUserId}");
            }

            return query.Where(x => x.Workspace.Members.Any(m => m.UserId == executingUserId));
        }
    }
}