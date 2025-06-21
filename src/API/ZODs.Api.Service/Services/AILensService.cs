using ZODs.Api.Common.Extensions;
using ZODs.Api.Common.Helpers;
using ZODs.Api.Common.Interfaces;
using ZODs.Api.Repository;
using ZODs.Api.Repository.Dtos.AILens;
using ZODs.Api.Repository.Entities.Entities;
using ZODs.Api.Repository.Interfaces;
using ZODs.Api.Repository.QueryParams;
using ZODs.Api.Service.Dtos;
using ZODs.Api.Service.InputDtos;
using ZODs.Api.Service.Mappers;
using ZODs.Common.Exceptions;
using ZODs.Common.Extensions;

namespace ZODs.Api.Service;

public sealed class AILensService : IAILensService
{
    private readonly IUnitOfWork<ZodsContext> unitOfWork;
    private readonly ICacheService cacheService;

    public AILensService(
        IUnitOfWork<ZodsContext> unitOfWork,
        ICacheService cacheService)
    {
        this.unitOfWork = unitOfWork;
        this.cacheService = cacheService;
    }

    public IAILensRepository AILensRepository => unitOfWork.GetRepository<IAILensRepository>();

    public async Task<AILensDto> GetLensByIdAsync(Guid lensId, Guid userId, CancellationToken cancellationToken)
    {
        var lens = await AILensRepository.FirstOrDefaultAsync(
            x => x.UserId == userId && x.Id == lensId,
            cancellationToken: cancellationToken).NoSync();

        if (lens == null)
        {
            throw new KeyNotFoundException($"AI Lens with id {lensId} not found.");
        }

        return lens.ToDto();
    }

    public async Task<ICollection<AILensInfoDto>> GetUserBuiltInLensesInfoAsync(
        Guid userId,
        bool tryGetFromCache = true,
        CancellationToken cancellationToken = default)
    {
        var builtInLensesInfoCacheKey = CacheKeyGenerator.BuiltInLenses;
        ICollection<AILensInfoDto>? userBuiltInLensesInfo = null;

        if (tryGetFromCache)
        {
            var cachedLensesInfo = await cacheService.Get<ICollection<AILensInfoDto>>(builtInLensesInfoCacheKey, cancellationToken);
            if (cachedLensesInfo is not null)
            {
                userBuiltInLensesInfo = cachedLensesInfo;
            }
        }

        if (userBuiltInLensesInfo == null)
        {
            // AI Lenses not found in cache, get from database
            userBuiltInLensesInfo = await AILensRepository.GetBuiltInLensesInfoAsync(cancellationToken).NoSync();
            if (tryGetFromCache)
            {
                // Cache AI Lenses
                await cacheService.Set(builtInLensesInfoCacheKey, userBuiltInLensesInfo, cancellationToken: cancellationToken);
            }
        }

        return userBuiltInLensesInfo;
    }

    public async Task<ICollection<AILensInfoDto>> GetUserAILensesInfoAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var allUserAILensesInfoCacheKey = CacheKeyGenerator.GetAllUserAILensesInfoKey(userId);
        var allUserLensesInfo = await cacheService.Get<ICollection<AILensInfoDto>>(allUserAILensesInfoCacheKey, cancellationToken);

        // If not found in cache, get from database
        if (allUserLensesInfo is null)
        {
            allUserLensesInfo = await AILensRepository.GetAILensesInfoAsync(userId, includeBuiltIn: true, cancellationToken).NoSync();

            // Important step - map built-in lenses settings
            // User can enable/disable built-in lenses and we need to map that
            // to the lenses info collection, as settings are not stored in separate table
            await MapBuiltInUserLensesSettingsAsync(allUserLensesInfo, userId, cancellationToken);

            await cacheService.Set(allUserAILensesInfoCacheKey, allUserLensesInfo, cancellationToken: cancellationToken);
        }

        return allUserLensesInfo;
    }

    public async Task<PagedResponse<UserAILensDto>> GatePagedUserAILensesAsync(
        GetPagedUserAILensesQuery query,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var pagedEntities = await AILensRepository.GetPagedUserAILensesAsync(
            query,
            userId,
            cancellationToken).NoSync();

        return pagedEntities.ToPagedResponse();
    }

    public async Task<PagedResponse<UserAILensDto>> GetPagedUserBuiltInAILensesAsync(
        GetUserBuiltInAILensesQuery query,
        Guid userId,
               CancellationToken cancellationToken)
    {
        var pagedEntities = await AILensRepository.GetPagedUserBuiltInAILensesAsync(
            query,
            userId,
            cancellationToken).NoSync();

        return pagedEntities.ToPagedResponse();
    }

    public async Task<AILensDto> CreateAILensAsync(
        AILensInputDto inputDto,
        Guid userId,
        CancellationToken cancellationToken)
    {
        await ValidateAILensDto(inputDto, userId, cancellationToken);

        var entity = inputDto.ToEntity();
        entity.UserId = userId;
        entity.IsEnabled = true;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = userId.ToString();

        await AILensRepository.Insert(entity, cancellationToken).NoSync();
        await unitOfWork.CommitAsync(cancellationToken).NoSync();

        // Invalidate cache
        await InvalidateUserAILensesCacheAsync(userId, cancellationToken);

        return entity.ToDto();
    }

    public async Task<AILensInputDto> UpdateAILensAsync(
        AILensInputDto inputDto,
        Guid userId,
        CancellationToken cancellationToken)
    {
        await ValidateAILensDto(inputDto, userId, cancellationToken);

        await AILensRepository.ExecuteUpdateAsync(
            inputDto.Id!.Value,
            setters => setters.SetProperty(lens => lens.Name, inputDto.Name)
                              .SetProperty(lens => lens.Tooltip, inputDto.Tooltip)
                              .SetProperty(lens => lens.TargetKind, inputDto.TargetKind)
                              .SetProperty(lens => lens.BehaviorInstruction, inputDto.BehaviorInstruction)
                              .SetProperty(lens => lens.ResponseInstruction, inputDto.ResponseInstruction)
                              .SetProperty(lens => lens.ModifiedAt, DateTime.UtcNow)
                              .SetProperty(lens => lens.ModifiedBy, userId.ToString()),
            cancellationToken);

        // Invalidate cache
        await InvalidateUserAILensesCacheAsync(userId, cancellationToken);

        return inputDto;
    }

    public async Task SetUserAILensEnabledAsync(
        Guid lenseId,
        Guid userId,
        bool isEnabled,
        CancellationToken cancellationToken = default)
    {
        await AILensRepository.SetUserAILensEnabledAsync(lenseId, userId, isEnabled, cancellationToken).NoSync();

        // Invalidate cache
        var allUserAILensesInfoCacheKey = CacheKeyGenerator.GetAllUserAILensesInfoKey(userId);
        await cacheService.Remove(allUserAILensesInfoCacheKey, cancellationToken);
    }

    public async Task<AILensInstructionsDto> GetAILensInstructionsAsync(
        Guid lensId,
        bool tryGetFromCache = true,
        CancellationToken cancellationToken = default)
    {
        var lensInstructionsCacheKey = CacheKeyGenerator.GetAILensInstructionsKey(lensId);
        AILensInstructionsDto? instructions = null;

        if (tryGetFromCache)
        {
            var cachedInstructions = await cacheService.Get<AILensInstructionsDto>(lensInstructionsCacheKey, cancellationToken);
            if (cachedInstructions is not null)
            {
                instructions = cachedInstructions;
            }
        }

        if (instructions == null)
        {
            instructions = await AILensRepository.GetAILensInstructionsAsync(lensId, cancellationToken);
            if (instructions == null)
            {
                throw new KeyNotFoundException(typeof(AILens).NotFoundValidationMessage(lensId));
            }

            if (tryGetFromCache)
            {
                await cacheService.Set(lensInstructionsCacheKey, instructions, cancellationToken: cancellationToken);
            }
        }

        return instructions;
    }

    public async Task<bool> AILensWithSameNameExists(
        CheckAILensNameExistsInputDto inputDto, 
        Guid userId,
        CancellationToken cancellationToken)
    {
        var exists = await AILensRepository.ExistsAsync(
                       x => x.UserId == userId
                            && x.Name == inputDto.Name
                            && (!inputDto.LensId.HasValue || x.Id != inputDto.LensId),
                                  cancellationToken: cancellationToken).NoSync();

        return exists;
    }

    private async Task ValidateAILensDto(AILensInputDto inputDto, Guid userId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(inputDto);

        if (inputDto.Id.HasValue)
        {
            var lensExists = await AILensRepository.ExistsAsync(x => x.UserId == userId && x.Id == inputDto.Id.Value, cancellationToken: cancellationToken).NoSync();

            if (!lensExists)
            {
                throw new KeyNotFoundException($"AI Lens with id {inputDto.Id} not found.");
            }
        }

        var duplicateAILensExists = await AILensRepository.ExistsAsync(
            x => x.UserId == userId &&
                 x.Name == inputDto.Name &&
                 (!inputDto.Id.HasValue || x.Id != inputDto.Id),
            cancellationToken: cancellationToken);

        if (duplicateAILensExists)
        {
            throw new DuplicateEntityException("AI Lens with the same name already exists.");
        }
    }

    private async Task MapBuiltInUserLensesSettingsAsync(
        ICollection<AILensInfoDto> userLenses,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var builtInLenses = userLenses.Where(l => l.IsBuiltIn).ToArray();
        var builtInLenseIds = builtInLenses.Select(l => l.Id).ToArray();
        var lensesEnabledMap = await AILensRepository.GetUserBuiltInAILensesEnabledAsync(builtInLenseIds, userId, cancellationToken);
        foreach (var lensInfo in builtInLenses)
        {
            if (lensesEnabledMap.TryGetValue(lensInfo.Id, out var isEnabled))
            {
                lensInfo.IsEnabled = isEnabled;
            }
        }
    }

    private async Task InvalidateUserAILensesCacheAsync(Guid userId, CancellationToken cancellationToken)
    {
        var allUserAILensesInfoCacheKey = CacheKeyGenerator.GetAllUserAILensesInfoKey(userId);
        await cacheService.Remove(allUserAILensesInfoCacheKey, cancellationToken);
    }
}