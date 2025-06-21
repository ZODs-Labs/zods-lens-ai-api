using ZODs.Api.Common.Extensions;
using ZODs.Api.Repository.Common;
using ZODs.Api.Repository.Dtos;
using ZODs.Api.Repository.Entities;
using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Repository.Extensions;
using ZODs.Api.Repository.Extensions.Queryable;
using ZODs.Api.Repository.QueryParams;
using ZODs.Common.Extensions;
using Microsoft.EntityFrameworkCore;

namespace ZODs.Api.Repository;

public sealed class WorkspacesRepository : Repository<Workspace, ZodsContext>, IWorkspacesRepository
{
    public WorkspacesRepository(ZodsContext context)
        : base(context)
    {
    }

    private IQueryable<Workspace> WorkspacesQuery => Context.Workspaces.Where(x => x.IsActive);

    public async Task<Workspace> GetByIdAsync(
         Guid id,
         Guid executingUserId,
         CancellationToken cancellationToken)
    {
        var entity = await WorkspacesQuery
            .BuildProtectedWorkspaceAccessQuery(executingUserId)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .NoSync();

        if (entity == null)
        {
            throw new KeyNotFoundException(typeof(Workspace).NotFoundValidationMessage(id));
        }

        return entity;
    }

    public async Task<ICollection<UserWorkspaceDto>> GetUserWorkspaces(
        Guid userId,
        GetUserWorkspacesQuery query,
        CancellationToken cancellationToken)
    {
        var entities = await WorkspacesQuery.BuildProtectedWorkspaceAccessQuery(userId)
                                               .Select(x => new UserWorkspaceDto
                                               {
                                                   Id = x.Id,
                                                   Name = x.Name,
                                                   Description = x.Description,
                                                   RoleIndex = x.Members.Where(m => m.UserId == userId)
                                                                        .Select(m => m.Roles.Select(r => r.Role.Index))
                                                                        .SelectMany(r => r)
                                                                        .First(),
                                                   LastUpdatedAt = x.ModifiedAt != null ? x.ModifiedAt.Value : x.CreatedAt,
                                               }).ToListAsync(cancellationToken).NoSync();

        foreach (var entity in entities)
        {
            entity.IsOwner = entity.RoleIndex == WorkspaceMemberRoleIndex.Owner;
        }

        return entities;
    }

    public async Task<ICollection<UserWorkspaceDto>> GetUserWorkspacesForWidgetAsync(
        GetUserWorkspacesQuery query,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var pagedEntities = await WorkspacesQuery.BuildProtectedWorkspaceAccessQuery(userId)
                                               .OrderByDescending(x => x.Members.Any(m => m.UserId == userId))
                                               .ThenBy(x => x.Members.Where(m => m.UserId == userId)
                                                                               .SelectMany(m => m.Roles.Select(r => r.Role.Index))
                                                                               .FirstOrDefault())
                                               .ThenByDescending(x => x.ModifiedAt != null ? x.ModifiedAt.Value : x.CreatedAt)
                                               .Select(x => new UserWorkspaceDto
                                               {
                                                   Id = x.Id,
                                                   Name = x.Name,
                                                   RoleIndex = x.Members.Where(m => m.UserId == userId)
                                                                        .Select(m => m.Roles.Select(r => r.Role.Index))
                                                                        .SelectMany(r => r)
                                                                        .First(),
                                                   LastUpdatedAt = x.ModifiedAt != null ? x.ModifiedAt.Value : x.CreatedAt,
                                               })
                                               .PageBy(query, cancellationToken).NoSync();

        foreach (var entity in pagedEntities.Entities)
        {
            entity.IsOwner = entity.RoleIndex == WorkspaceMemberRoleIndex.Owner;
        }

        return pagedEntities.Entities;
    }

    public async Task<PagedEntities<WorkspaceMemberDto>> GetPagedWorkspaceMembersAsync(
        Guid workspaceId,
        Guid userId,
        GetPagedWorkspaceMembersQuery query,
        CancellationToken cancellationToken)
    {
        var queryable = this.Context.WorkspaceMembers
                                    .BuildProtectedWorkspaceAccessQuery(userId)
                                    .Include(x => x.Roles)
                                    .ContainsSearchTerm(
                                           query.SearchTerm,
                                           $"{nameof(User)}.{nameof(WorkspaceMember.User.FirstName)}",
                                           $"{nameof(User)}.{nameof(WorkspaceMember.User.LastName)}",
                                           $"{nameof(User)}.{nameof(WorkspaceMember.User.Email)}")
                                    .Where(x => x.WorkspaceId == workspaceId);

        Func<IQueryable<WorkspaceMember>, IQueryable<WorkspaceMemberDto>> transformMapper =
           wmQueryable => wmQueryable.Select(x => new WorkspaceMemberDto
           {
               Id = x.Id,
               UserId = x.UserId,
               FirstName = x.User.FirstName,
               LastName = x.User.LastName,
               EmailAddress = x.User.Email ?? string.Empty,

               WorkspaceRoles = x.Roles.Select(r => new WorkspaceRoleDto
               {
                   Name = r.Role.Name,
                   Index = r.Role.Index,
               }),
           });

        var pagedEntities = await queryable.PageBy(
            transformQuery: transformMapper,
            query,
            cancellationToken)
            .NoSync();

        return pagedEntities;
    }

    public async Task<int> GetAllUserWorkspacesSnippetVersionSumAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var versionSum = await Context.WorkspaceSnippetsVersions
             .Where(x => x.Workspace.Members.Any(m => m.UserId == userId))
             .Select(x => x.SnippetsVersion)
             .SumAsync(cancellationToken)
             .NoSync();

        return versionSum;
    }

    public async Task IncrementWorksaceSnippetsVersionAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        var snippetsVersionExists = await this.Context.WorkspaceSnippetsVersions
            .AnyAsync(x => x.WorkspaceId == workspaceId,
                      cancellationToken)
            .NoSync();

        if (snippetsVersionExists)
        {
            await this.Context.WorkspaceSnippetsVersions
                .Where(x => x.WorkspaceId == workspaceId)
                .ExecuteUpdateAsync(
                       setter => setter.SetProperty(x => x.SnippetsVersion, x => x.SnippetsVersion + 1),
                       cancellationToken)
                .NoSync();
        }
        else
        {
            var entity = new WorkspaceSnippetsVersion
            {
                WorkspaceId = workspaceId,
                SnippetsVersion = 1,
            };

            await this.Context.WorkspaceSnippetsVersions.AddAsync(entity, cancellationToken);
        }
    }

    public async Task<bool> IsUserWorkspaceOwnerAsync(Guid userId, Guid workspaceId, CancellationToken cancellationToken)
    {
        var isOwner = await WorkspacesQuery
            .AnyAsync(x => x.Id == workspaceId &&
                           x.Members.Any(m => m.UserId == userId &&
                                              m.Roles.Any(r => r.Role.Index == WorkspaceMemberRoleIndex.Owner)),
                      cancellationToken)
            .NoSync();

        return isOwner;
    }

    public async Task<bool> IsUserWorkspaceMemberAsync(Guid userId, Guid workspaceId, CancellationToken cancellationToken)
    {
        var isMember = await this.Context.WorkspaceMembers
            .AnyAsync(x => x.WorkspaceId == workspaceId &&
                           x.UserId == userId,
                      cancellationToken)
            .NoSync();

        return isMember;
    }

    public async Task<bool> IsWorkspaceRoleLowerThanUserWorkspaceRole(
        Guid workspaceId,
        Guid userId,
        WorkspaceMemberRoleIndex roleIndex,
        CancellationToken cancellationToken)
    {
        // Looking for role that has lower value
        // because the lower the value the higher the role
        // according to the enum `WorkspaceMemberRoleIndex`
        var isLower = await this.Context.WorkspaceMembers
            .AnyAsync(x => x.WorkspaceId == workspaceId &&
                           x.UserId == userId &&
                           x.Roles.Any(r => r.Role.Index < roleIndex),
                      cancellationToken)
            .NoSync();

        return isLower;
    }

    public async Task<Guid> GetRoleIdByIndexAsync(WorkspaceMemberRoleIndex roleIndex, CancellationToken cancellationToken)
    {
        var roleId = await this.Context.WorkspaceRoles
            .Where(x => x.Index == roleIndex)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken)
            .NoSync();

        return roleId;
    }

    public async Task AddMemberWithRoleToWorkspaceAsync(
        WorkspaceMember workspaceMember,
        CancellationToken cancellationToken)
    {
        await this.Context.WorkspaceMembers.AddAsync(workspaceMember, cancellationToken);
    }

    public async Task<ICollection<WorkspaceDropdownDto>> GetUserWorkspacesDropdownAsync(
        Guid userId,
        WorkspaceMemberRoleIndex userRoleIndex,
        CancellationToken cancellationToken)
    {
        var entities = await Context.Workspaces.Where(x => x.Members.Any(m => m.UserId == userId &&
                                                                              m.Roles.Any(r => r.Role.Index == userRoleIndex)))
                                               .Select(x => new WorkspaceDropdownDto
                                               {
                                                   Id = x.Id,
                                                   Name = x.Name,
                                               }).ToArrayAsync(cancellationToken).NoSync();

        return entities;
    }

    public async Task RemoveWorkspaceMemberAsync(
        Guid workspaceMemberId,
        CancellationToken cancellationToken)
    {
        await this.Context.WorkspaceMembers
            .Where(x => x.Id == workspaceMemberId)
            .ExecuteDeleteAsync(cancellationToken)
            .NoSync();
    }

    public async Task<bool> UserScopedWorkspaceWithSameNameExistsAsync(
        string name,
        Guid userId,
        Guid? ignoreWorkspaceId,
        CancellationToken cancellationToken = default)
    {
        var exists = await this.Context.Workspaces.BuildProtectedWorkspaceAccessQuery(userId)
            .AnyAsync(x => x.Id != ignoreWorkspaceId &&
                           EF.Functions.ILike(x.Name, name),
                      cancellationToken)
            .NoSync();

        return exists;
    }

    public async Task<int> GetUserCreatedWorkspacesCount(Guid userId, CancellationToken cancellationToken = default)
    {
        var count = await this.Context.Workspaces
            .Where(x => x.Members.Any(m => m.UserId == userId &&
                                           m.Roles.Any(r => r.Role.Index == WorkspaceMemberRoleIndex.Owner)))
            .CountAsync(cancellationToken)
            .NoSync();

        return count;
    }

    public async Task<IDictionary<Guid, int>> CountSnippetsByWorkspacesForUser(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var resultList = await (
            from workspaceMember in this.Context.WorkspaceMembers
            where workspaceMember.UserId == userId &&
                  workspaceMember.Roles.Any(r => r.Role.Index == WorkspaceMemberRoleIndex.Owner)
            join snippet in this.Context.Snippets
              on workspaceMember.WorkspaceId equals snippet.WorkspaceId into snippetGroup
            from sg in snippetGroup.DefaultIfEmpty()
            group sg by workspaceMember.WorkspaceId into grouped
            select new
            {
                WorkspaceId = grouped.Key,
                SnippetCount = grouped.Count(s => s != null)
            }
        ).ToListAsync(cancellationToken);

        var resultDictionary = resultList.ToDictionary(x => x.WorkspaceId, x => x.SnippetCount);

        return resultDictionary;
    }

    public async Task<IDictionary<Guid, int>> CountInvitesByWorksapcesForUser(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var resultList = await (
            from workspaceMember in this.Context.WorkspaceMembers
            where workspaceMember.UserId == userId &&
                  workspaceMember.Roles.Any(r => r.Role.Index == WorkspaceMemberRoleIndex.Owner)
            join invite in this.Context.WorkspaceMemberInvites
              on workspaceMember.WorkspaceId equals invite.WorkspaceId into inviteGroup
            from ig in inviteGroup.DefaultIfEmpty()
            group ig by workspaceMember.WorkspaceId into grouped
            select new
            {
                WorkspaceId = grouped.Key,
                InviteCount = grouped.Count(i => i != null)
            }
        ).ToListAsync(cancellationToken);

        var resultDictionary = resultList.ToDictionary(x => x.WorkspaceId, x => x.InviteCount);

        return resultDictionary;
    }


    public async Task DeleteAllUserOwnedWorkspacesAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        // 1) Delete all workspace member roles where the user is owner
        await this.Context.WorkspaceMemberRoles
            .Where(x => x.WorkspaceMember.Workspace.Members.Any(m => m.UserId == userId &&
                                                                     m.Roles.Any(r => r.Role.Index == WorkspaceMemberRoleIndex.Owner)))
            .ExecuteDeleteAsync(cancellationToken)
            .NoSync();

        // 2) Delete all workspaces members where the user is owner
        await this.Context.WorkspaceMembers
            .Where(x => x.Workspace.Members.Any(m => m.UserId == userId &&
                                                     m.Roles.Any(r => r.Role.Index == WorkspaceMemberRoleIndex.Owner)))
            .ExecuteDeleteAsync(cancellationToken)
            .NoSync();

        // 3) Delete all workpsace member invites where the user is owner
        await this.Context.WorkspaceMemberInvites
            .Where(x => x.Workspace.Members.Any(m => m.UserId == userId &&
                                                     m.Roles.Any(r => r.Role.Index == WorkspaceMemberRoleIndex.Owner)))
            .ExecuteDeleteAsync(cancellationToken)
            .NoSync();

        // 4) Delete all workspace snippets where the user is owner
        await this.Context.Snippets
            .Where(x => x.WorkspaceId != null && x.Workspace!.Members.Any(m => m.UserId == userId &&
                                                                               m.Roles.Any(r => r.Role.Index == WorkspaceMemberRoleIndex.Owner)))
            .ExecuteDeleteAsync(cancellationToken)
            .NoSync();

        // 5) Delete all workspaces where the user is owner
        await this.Context.Workspaces
            .Where(x => x.Members.Any(m => m.UserId == userId &&
                                                      m.Roles.Any(r => r.Role.Index == WorkspaceMemberRoleIndex.Owner)))
            .ExecuteDeleteAsync(cancellationToken)
            .NoSync();
    }

    public async Task RemoveMemberFromAllWorkspacesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await this.Context.WorkspaceMembers
            .Where(x => x.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken)
            .NoSync();
    }

    public async Task<ICollection<Guid>> GetAllExclusiveMembersOfOwnerWorkspacesAsync(
        Guid userOwnerId,
        CancellationToken cancellationToken = default)
    {
        var workspaceIds = await this.Context.WorkspaceMembers
            .Where(x => x.Workspace.Members.Any(m => m.UserId == userOwnerId &&
                                                     m.Roles.Any(r => r.Role.Index == WorkspaceMemberRoleIndex.Owner)))
            .Select(x => x.WorkspaceId)
            .ToArrayAsync(cancellationToken);

        var memberUserIds = await Context.WorkspaceMemberInvites
            .Where(x => workspaceIds.Contains(x.WorkspaceId) &&
                        x.IsUsed &&
                        x.AcceptedByUserId != null &&
                        !x.AcceptedByUser!.Workspaces.Any(w => !workspaceIds.Contains(w.WorkspaceId) &&
                                                               w.Workspace.IsActive))
            .Distinct()
            .Select(x => x.AcceptedByUserId!.Value)
            .ToArrayAsync(cancellationToken);

        return memberUserIds;
    }

    public async Task SetUserWorkspacesActiveStatusAsync(
        Guid userId,
        bool isActive,
        CancellationToken cancellationToken)
    {
        await Context.Workspaces.BuildProtectedOwnerWorkspacesQuery(userId)
                                .ExecuteUpdateAsync(
                                    setter => setter.SetProperty(x => x.IsActive, isActive),
                                    cancellationToken)
                                .NoSync();
    }

    public async Task<Dictionary<Guid, string>> GetWorkspacesNameByIdsAsync(
        ICollection<Guid> workspaceIds,
        CancellationToken cancellationToken)
    {
        var workspaceNames = await Context.Workspaces
                                          .Where(x => workspaceIds.Contains(x.Id))
                                          .Select(x => new
                                          {
                                              x.Name,
                                              x.Id,
                                          })
                                          .ToDictionaryAsync(
                                            x => x.Id,
                                            x => x.Name,
                                            cancellationToken)
                                          .NoSync();

        return workspaceNames;
    }
}
