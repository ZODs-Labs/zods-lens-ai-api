using ZODs.Api.Repository.Common;
using ZODs.Api.Repository.Dtos;
using ZODs.Api.Repository.Entities;
using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Repository.Extensions;
using ZODs.Api.Repository.Extensions.Queryable;
using ZODs.Api.Repository.QueryParams;
using ZODs.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data;

namespace ZODs.Api.Repository;

public class SnippetsRepository : Repository<Snippet, ZodsContext>, ISnippetsRepository
{
    public SnippetsRepository(ZodsContext context) : base(context)
    {
    }

    public async Task<Snippet?> GetUserSnippetByIdAsync(
        Guid snippetId,
        Guid executingUserId,
        CancellationToken cancellationToken)
    {
        var snippet = await Context.Snippets.BuildProtectedSnippetReadAccessQuery(executingUserId)
                                            .FirstOrDefaultAsync(x => x.Id == snippetId, cancellationToken)
                                            .NoSync();
        return snippet;
    }

    public async Task<PagedEntities<SnippetOverviewDto>> GetUserSnippetsOverviewAsync(Guid userId, GetUserSnippetsQuery query, CancellationToken cancellationToken)
    {
        var queryable = Context.Snippets.BuildProtectedSnippetReadAccessQuery(userId)
                                        .Include(x => x.TriggerPrefix)
                                        .WithSearchTerm(query.SearchTerm);

        var pagedEntities = await queryable
        .PageBy(
                transformQuery: GetSnippetOverviewTransformMapper(userId),
                query!,
                cancellationToken)
            .NoSync();

        return pagedEntities;
    }

    public async Task<PagedEntities<SnippetOverviewDto>> GetSnippetsSharedWithUser(
        Guid userId,
        GetUserSnippetsQuery query,
        CancellationToken cancellationToken)
    {
        // Only snippets shared with the workspaces the user is member of.
        var queryable = Context.Snippets.Where(x => x.SharedWorkspaces.Any(w => w.Workspace!.Members.Any(m => m.UserId == userId)) ||
                                                    (x.IsWorkspaceOwned && x.Workspace!.Members.Any(m => m.UserId == userId)))
                                        .Include(x => x.TriggerPrefix)
                                        .WithSearchTerm(query.SearchTerm);

        var pagedEntities = await queryable
        .PageBy(
                transformQuery: GetSnippetOverviewTransformMapper(userId),
                query!,
                cancellationToken)
            .NoSync();

        return pagedEntities;
    }

    public async Task<PagedEntities<SnippetOverviewDto>> GetUserOwnSnippets(
        Guid userId,
        GetUserSnippetsQuery query,
        CancellationToken cancellationToken)
    {
        var queryable = Context.Snippets.Where(x => !x.IsWorkspaceOwned && x.UserId == userId)
                                        .Include(x => x.TriggerPrefix)
                                        .WithSearchTerm(query.SearchTerm);

        var pagedEntities = await queryable
            .PageBy(
                transformQuery: GetSnippetOverviewTransformMapper(userId),
                query!,
                cancellationToken)
            .NoSync();

        return pagedEntities;
    }

    public async Task<ICollection<SnippetForIntegrationDto>> GetUserSnippetsForIntegrationAsync(Guid userId, CancellationToken cancellationToken)
    {
        var userSnippets = await Context.Snippets.BuildProtectedSnippetReadAccessQuery(userId)
            .Include(x => x.TriggerPrefix)
            .Select(x => new SnippetForIntegrationDto
            {
                Name = x.Name,
                Description = x.Description,
                Trigger = x.Trigger,
                Language = x.Language,
                CodeSnippet = x.CodeSnippet,
            })
            .ToArrayAsync(cancellationToken);

        return userSnippets;
    }

    public async Task<PagedEntities<SnippetOverviewDto>> GetWorkspaceSnippetsOverviewAsync(
        Guid workspaceId,
        Guid executingUserId,
        GetWorkspaceSnippetsQuery query,
        CancellationToken cancellationToken)
    {
        var queryable = Context.Snippets.Where(x => x.WorkspaceId == workspaceId)
                                        .Include(x => x.TriggerPrefix)
                                        .WithSearchTerm(query.SearchTerm);

        var pagedEntities = await queryable
            .PageBy(
                transformQuery: GetSnippetOverviewTransformMapper(executingUserId),
                query!,
                cancellationToken)
            .NoSync();

        return pagedEntities;
    }

    public async Task<Snippet> CreateSnippetAsync(Snippet snippet, Guid? ownerWorkspaceId, CancellationToken cancellationToken)
    {
        if (snippet.IsWorkspaceOwned && ownerWorkspaceId.HasValue)
        {
            snippet = AssignOwnerWorkspace(snippet, ownerWorkspaceId.Value);
        }

        await this.Insert(snippet, cancellationToken).NoSync();

        return snippet;
    }

    public async Task<Snippet> UpdateSnippetAsync(Snippet snippet, Guid? ownerWorkspaceId, CancellationToken cancellationToken)
    {
        if (snippet.IsWorkspaceOwned && ownerWorkspaceId.HasValue)
        {
            snippet = AssignOwnerWorkspace(snippet, ownerWorkspaceId.Value);

            await this.Context.WorkspaceSnippets.Where(x => x.SnippetId == snippet.Id && x.WorkspaceId != ownerWorkspaceId)
                                         .ExecuteDeleteAsync(cancellationToken)
                                         .NoSync();
        }

        await this.Update(snippet, cancellationToken).NoSync();
        return snippet;
    }

    public async Task IncrementUserSnippetsVersion(Guid userId, CancellationToken cancellationToken)
    {
        var userSnippetsVersionExists = await Context.UserSnippetsVersions.AnyAsync(x => x.UserId == userId, cancellationToken).NoSync();

        if (userSnippetsVersionExists)
        {
            await Context.UserSnippetsVersions
                .ExecuteUpdateAsync(
                    setter => setter.SetProperty(x => x.SnippetVersion, x => x.SnippetVersion + 1),
                    cancellationToken);
        }
        else
        {
            var entity = new UserSnippetsVersion
            {
                UserId = userId,
                SnippetVersion = 1,
            };

            await Context.UserSnippetsVersions.AddAsync(entity, cancellationToken);
        }
    }

    public async Task<long> GetUserSnippetsVersionAsync(Guid userId, CancellationToken cancellationToken)
    {
        var version = await this.Context.UserSnippetsVersions.Where(x => x.UserId == userId)
                                                              .Select(x => x.SnippetVersion)
                                                              .FirstOrDefaultAsync(cancellationToken)
                                                              .NoSync();

        return version;
    }

    public async Task<string> GetSnippetCodeAsync(Guid snippetId, Guid userId, CancellationToken cancellationToken)
    {
        var snippetCode = await Context.Snippets.BuildProtectedSnippetReadAccessQuery(userId)
                                                .Where(x => x.Id == snippetId)
                                                .Select(x => x.CodeSnippet)
                                                .FirstOrDefaultAsync(cancellationToken)
                                                .NoSync() ?? string.Empty;

        return snippetCode;
    }

    public async Task<bool> UserSnippetExists(Guid snippetId, Guid userId, CancellationToken cancellationToken)
    {
        var snippetExists = await Context.Snippets.BuildProtectedSnippetReadAccessQuery(userId)
                                                  .AnyAsync(x => x.Id == snippetId, cancellationToken)
                                                  .NoSync();

        return snippetExists;
    }

    public async Task<bool> HasUserSnippetWritePermission(Guid snippetId, Guid userId, CancellationToken cancellationToken)
    {
        // User has snippet write permission only if he is the owner of the snippet
        // or he is owner of the workspace in which the snippet is.
        var hasWritePermissionForSnippet = await Context.Snippets.AnyAsync(
            x => (!x.IsWorkspaceOwned && x.UserId == userId) ||
                 x.Workspace!.Members.Any(m => m.UserId == userId &&
                                              m.Roles.Any(r => r.Role.Index == WorkspaceMemberRoleIndex.Owner)),
            cancellationToken).NoSync();

        return hasWritePermissionForSnippet;
    }

    public async Task<bool> DuplicateSnippetExists(
        string name,
        string trigger,
        Guid? snippetId,
        Guid? workspaceId,
        Guid userId,
        bool isWorkspaceOwned,
        CancellationToken cancellationToken)
    {
        var queryable = Context.Snippets.AsQueryable();
        if (snippetId.HasValue)
        {
            queryable = queryable.Where(x => x.Id != snippetId.Value);
        }

        if (isWorkspaceOwned)
        {
            queryable = queryable.Where(x => x.IsWorkspaceOwned && x.WorkspaceId == workspaceId);
        }
        else
        {
            queryable = queryable.Where(x => !x.IsWorkspaceOwned && x.UserId == userId);
        }

        var duplicateNameExists = await queryable.AnyAsync(
            x => EF.Functions.ILike(x.Name, $"%{name}%"),
            cancellationToken).NoSync();

        return duplicateNameExists;
    }

    public async Task<int> GetUserOwnSnippetsCountAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var userSnippetsCount = await Context.Snippets.Where(x => !x.IsWorkspaceOwned && x.UserId == userId)
                                                      .CountAsync(cancellationToken)
                                                      .NoSync();

        return userSnippetsCount;
    }

    public async Task DeleteAllUserSnippetsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        await Context.Snippets.Where(x => !x.IsWorkspaceOwned && x.UserId == userId)
                              .ExecuteDeleteAsync(cancellationToken)
                              .NoSync();
    }

    private static Snippet AssignOwnerWorkspace(Snippet snippet, Guid workspaceId)
    {
        snippet.IsWorkspaceOwned = true;
        snippet.WorkspaceId = workspaceId;

        return snippet;
    }

    /// <summary>
    /// Transforms an IQueryable of Snippet to an IQueryable of SnippetOverviewDto.
    /// </summary>
    /// <param name="wmQueryable">An IQueryable collection of Snippet.</param>
    /// <param name="userId">The user ID for which the editability should be checked.</param>
    /// <returns>An IQueryable collection of SnippetOverviewDto.</returns>
    private static Func<IQueryable<Snippet>, IQueryable<SnippetOverviewDto>> GetSnippetOverviewTransformMapper(Guid userId)
    {
        IQueryable<SnippetOverviewDto> transformMapper(IQueryable<Snippet> wmQueryable) => wmQueryable.Select(x => new SnippetOverviewDto
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            Trigger = x.Trigger,
            Language = x.Language,
            CreatedAt = x.CreatedAt,
            IsEditable = (x.UserId == userId && !x.IsWorkspaceOwned) ||
                         x.Workspace!.Members.Any(m => m.UserId == userId &&
                                                      m.Roles.Any(r => r.Role.Index == WorkspaceMemberRoleIndex.Owner)),
        });

        return transformMapper;
    }
}
