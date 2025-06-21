using Microsoft.EntityFrameworkCore;
using ZODs.Api.Repository.Common;
using ZODs.Api.Repository.Dtos.AILens;
using ZODs.Api.Repository.Entities;
using ZODs.Api.Repository.Entities.Entities;
using ZODs.Api.Repository.Extensions;
using ZODs.Api.Repository.Extensions.Queryable;
using ZODs.Api.Repository.QueryParams;
using ZODs.Common.Extensions;
using System.Linq.Dynamic.Core;

namespace ZODs.Api.Repository;

public sealed class AILensRepository(ZodsContext context) : Repository<AILens, ZodsContext>(context), IAILensRepository
{
    public async Task<AILensInstructionsDto?> GetAILensInstructionsAsync(
        Guid lensId,
        CancellationToken cancellationToken = default)
    {
        var lensInstructions = await Context.AILenses
            .AsNoTracking()
            .Where(l => l.Id == lensId)
            .Select(l => new AILensInstructionsDto
            {
                BehaviorInstruction = l.BehaviorInstruction,
                ResponseInstruction = l.ResponseInstruction
            })
            .FirstOrDefaultAsync(cancellationToken);

        return lensInstructions;
    }

    public async Task<ICollection<AILensInfoDto>> GetBuiltInLensesInfoAsync(
        CancellationToken cancellationToken = default)
    {
        var lensesInfo = await Context.AILenses
            .Where(x => x.IsBuiltIn)
            .Select(x => new AILensInfoDto
            {
                Id = x.Id,
                Name = x.Name,
                Tooltip = x.Tooltip,
                TargetKind = x.TargetKind,
                IsEnabled = x.IsEnabled,
                IsBuiltIn = x.IsBuiltIn,
            })
            .ToArrayAsync(cancellationToken);

        return lensesInfo;
    }

    public async Task<ICollection<AILensInfoDto>> GetAILensesInfoAsync(
        Guid userId,
        bool includeBuiltIn = true,
        CancellationToken cancellationToken = default)
    {
        var lensesInfo = await Context.AILenses
            .Where(x => x.UserId == userId || (includeBuiltIn && x.IsBuiltIn))
            .Select(x => new AILensInfoDto
            {
                Id = x.Id,
                Name = x.Name,
                Tooltip = x.Tooltip,
                TargetKind = x.TargetKind,
                IsBuiltIn = x.IsBuiltIn,
                IsEnabled = x.IsEnabled,
                UserId = x.UserId,
                WorkspaceId = x.WorkspaceId,
            })
            .ToArrayAsync(cancellationToken);

        return lensesInfo;
    }

    public async Task<Dictionary<Guid, bool>> GetUserBuiltInAILensesEnabledAsync(
        ICollection<Guid> builtInAILenseIds,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (builtInAILenseIds.Count == 0)
        {
            return [];
        }

        var userBuiltInAILensesEnabled = await Context.UserAILensSettings
            .Where(x => x.UserId == userId && builtInAILenseIds.Contains(x.AILensId))
            .ToDictionaryAsync(x => x.AILensId, x => x.IsEnabled, cancellationToken).NoSync();

        return userBuiltInAILensesEnabled;
    }

    public async Task<PagedEntities<UserAILensDto>> GetPagedUserAILensesAsync(
        GetPagedUserAILensesQuery query,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var queryable = Context.AILenses.BuildProtectedAILensReadAccessQuery(userId)
            .Where(x => !x.IsBuiltIn)
            .ContainsSearchTerm(
                query.SearchTerm,
                nameof(AILens.Name),
                nameof(AILens.Tooltip),
                nameof(AILens.BehaviorInstruction),
                nameof(AILens.ResponseInstruction));

        var pagedEntities = await queryable
            .PageBy(
               transformQuery: PagedUserAILensesTransformMapper,
               query!,
               cancellationToken: cancellationToken)
            .NoSync();

        return pagedEntities;
    }

    public async Task<PagedEntities<UserAILensDto>> GetPagedUserBuiltInAILensesAsync(
        GetUserBuiltInAILensesQuery query,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var queryable = Context.AILenses.BuildProtectedAILensReadAccessQuery(userId)
            .Where(x => x.IsBuiltIn)
            .ContainsSearchTerm(
                query.SearchTerm,
                nameof(AILens.Name),
                nameof(AILens.Tooltip),
                nameof(AILens.BehaviorInstruction),
                nameof(AILens.ResponseInstruction));

        var pagedEntities = await queryable
            .PageBy(
               transformQuery: PagedUserAILensesTransformMapper,
               query!,
               cancellationToken: cancellationToken)
            .NoSync();

        var userBuiltInAILensesEnabled = await GetUserBuiltInAILensesEnabledAsync(
            pagedEntities.Entities.Select(x => x.Id).ToArray(),
            userId,
            cancellationToken);

        foreach (var entity in pagedEntities.Entities)
        {
            if (userBuiltInAILensesEnabled.TryGetValue(entity.Id, out var isEnabled))
            {
                entity.IsEnabled = isEnabled;
            }
        }

        return pagedEntities;
    }

    public async Task SetUserAILensEnabledAsync(
        Guid lenseId,
        Guid userId,
        bool isEnabled,
        CancellationToken cancellationToken = default)
    {
        var isBuiltIn = await Context.AILenses
            .Where(x => x.Id == lenseId)
            .Select(x => x.IsBuiltIn)
            .FirstOrDefaultAsync(cancellationToken);

        if (isBuiltIn)
        {
            var updatedRecords = await Context.UserAILensSettings.Where(
                 x => x.UserId == userId && x.AILensId == lenseId)
                 .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsEnabled, isEnabled), cancellationToken).NoSync();

            if (updatedRecords == 0)
            {
                await Context.UserAILensSettings.AddAsync(new UserAILensSettings
                {
                    UserId = userId,
                    AILensId = lenseId,
                    IsEnabled = isEnabled,
                }, cancellationToken);
                await Context.SaveChangesAsync(cancellationToken).NoSync();
            }
        }
        else
        {
            await Context.AILenses.Where(
                   x => x.Id == lenseId && x.UserId == userId)
                .ExecuteUpdateAsync(
                   x => x.SetProperty(y => y.IsEnabled, isEnabled),
                   cancellationToken).NoSync();
        }
    }

    public Task<int> GetUserOwnAILensesCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Context.AILenses
            .Where(x => x.UserId == userId && !x.IsBuiltIn)
            .CountAsync(cancellationToken);
    }

    private static Func<IQueryable<AILens>, IQueryable<UserAILensDto>> PagedUserAILensesTransformMapper =>
        (IQueryable<AILens> queryable)
            => queryable
                .Select(x => new UserAILensDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Tooltip = x.Tooltip,
                    TargetKind = x.TargetKind,
                    IsEnabled = x.IsEnabled,
                    IsBuiltIn = x.IsBuiltIn,
                    CreatedAt = x.CreatedAt,
                });
}