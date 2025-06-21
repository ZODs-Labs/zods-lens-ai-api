using Microsoft.Extensions.Logging;
using ZODs.AI.OpenAI.Utils;
using ZODs.Api.Common.Helpers;
using ZODs.Api.Common.Interfaces;
using ZODs.Api.Repository;
using ZODs.Api.Repository.Dtos;
using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Repository.Interfaces;
using ZODs.Common.Extensions;

namespace ZODs.Api.Service;

public sealed class UserAICreditBalanceService : IUserAICreditBalanceService
{
    private readonly IUnitOfWork<ZodsContext> unitOfWork;
    private readonly ICacheService cacheService;
    private readonly ILogger<UserAICreditBalanceService> logger;

    public UserAICreditBalanceService(
        IUnitOfWork<ZodsContext> unitOfWork,
        ICacheService cacheService,
        ILogger<UserAICreditBalanceService> logger)
    {
        this.unitOfWork = unitOfWork;
        this.cacheService = cacheService;
        this.logger = logger;
    }

    private IUserAICreditBalanceRepository UserAICreditBalanceRepository => unitOfWork.GetRepository<IUserAICreditBalanceRepository>();

    public async Task<int> GetUserGpt3CreditBalanceAsync(Guid userId, CancellationToken cancellationToken)
    {
        var userGpt3CreditBalanceCacheKey = CacheKeyGenerator.GetUserGpt3CreditsBalanceKey(userId);
        var userGpt3CreditBalance = await cacheService.GetInteger(userGpt3CreditBalanceCacheKey, cancellationToken: cancellationToken);

        if (userGpt3CreditBalance == null)
        {
            // No cache found, get from database
            userGpt3CreditBalance = await UserAICreditBalanceRepository.FirstOrDefaultAsync(
                x => x.UserId == userId,
                selector: x => x.Gpt3Credits,
                cancellationToken: cancellationToken);

            // Update cache
            await UpdateUserGpt3CreditBalanceCache(userId, userGpt3CreditBalance.Value, cancellationToken);
        }

        return userGpt3CreditBalance.Value;
    }

    public async Task<int> GetUserGpt4CreditBalanceAsync(Guid userId, CancellationToken cancellationToken)
    {
        var userGpt4CreditBalanceCacheKey = CacheKeyGenerator.GetUserGpt4CreditsBalanceKey(userId);
        var userGpt4CreditBalance = await cacheService.GetInteger(userGpt4CreditBalanceCacheKey, cancellationToken: cancellationToken);

        if (userGpt4CreditBalance == null)
        {
            // No cache found, get from database
            userGpt4CreditBalance = await UserAICreditBalanceRepository.FirstOrDefaultAsync(
                x => x.UserId == userId,
                selector: x => x.Gpt4Credits,
                cancellationToken: cancellationToken);

            // Update cache
            await UpdateUserGpt4CreditBalanceCache(userId, userGpt4CreditBalance.Value, cancellationToken);
        }

        return userGpt4CreditBalance.Value;
    }

    public async Task AddUserGpt3CreditsAsync(Guid userId, int credits, CancellationToken cancellationToken)
    {
        await UserAICreditBalanceRepository.AddUserGpt3CreditsAsync(userId, credits, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        var gpt3CreditsBalance = await UserAICreditBalanceRepository.FirstOrDefaultAsync(
            x => x.UserId == userId,
            selector: x => x.Gpt3Credits,
            cancellationToken: cancellationToken);
        await UpdateUserGpt3CreditBalanceCache(userId, gpt3CreditsBalance, cancellationToken);
    }

    public async Task AddUserGpt4CreditsAsync(Guid userId, int credits, CancellationToken cancellationToken)
    {
        await UserAICreditBalanceRepository.AddUserGpt4CreditsAsync(userId, credits, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        var gpt4CreditsBalance = await UserAICreditBalanceRepository.FirstOrDefaultAsync(
            x => x.UserId == userId,
            selector: x => x.Gpt4Credits,
            cancellationToken: cancellationToken);
        await UpdateUserGpt4CreditBalanceCache(userId, gpt4CreditsBalance, cancellationToken);
    }

    public async Task UseUserGpt3CreditsAsync(Guid userId, int credits, CancellationToken cancellationToken)
    {
        var gpt3CreditsLeft = await UserAICreditBalanceRepository.UseUserGpt3CreditsAsync(userId, credits, cancellationToken);
        if (gpt3CreditsLeft < 0)
        {
            // If credits overflow happens, log it as CRITICAL and set credits to 0
            logger.LogCritical("GPT-3 AI Credits Overflow: User {UserId} has {Credits} credits left and used {usedCredits}", userId, gpt3CreditsLeft, credits);
            await UserAICreditBalanceRepository.SetUserGpt4CreditsAsync(userId, 0, cancellationToken);

            gpt3CreditsLeft = 0;
        }

        await UpdateUserGpt3CreditBalanceCache(userId, gpt3CreditsLeft, cancellationToken);
    }

    public async Task UseUserGpt4CreditsAsync(Guid userId, int credits, CancellationToken cancellationToken)
    {
        var gpt4CreditsLeft = await UserAICreditBalanceRepository.UseUserGpt4CreditsAsync(userId, credits, cancellationToken);

        if (gpt4CreditsLeft < 0)
        {
            // If credits overflow happens, log it as CRITICAL and set credits to 0
            logger.LogCritical("GPT-4 AI Credits Overflow: User {UserId} has {Credits} credits left and used {usedCredits}", userId, gpt4CreditsLeft, credits);
            await UserAICreditBalanceRepository.SetUserGpt4CreditsAsync(userId, 0, cancellationToken);

            gpt4CreditsLeft = 0;
        }

        await UpdateUserGpt4CreditBalanceCache(userId, gpt4CreditsLeft, cancellationToken);
    }

    public async Task<UserAICreditsBalanceDto> GetUserAICreditsBalanceAsync(Guid userId, CancellationToken cancellationToken)
    {
        var userAICreditsBalance = await UserAICreditBalanceRepository.FirstOrDefaultAsync(
              x => x.UserId == userId,
              selector: x => new UserAICreditsBalanceDto
              {
                  Gpt3Credits = x.Gpt3Credits,
                  Gpt4Credits = x.Gpt4Credits,
              },
              cancellationToken: cancellationToken)
            .NoSync();

        if (userAICreditsBalance == null)
        {
            return new UserAICreditsBalanceDto
            {
                Gpt3Credits = 0,
                Gpt4Credits = 0,
            };
        }

        return userAICreditsBalance;
    }

    public async Task SetUserAICreditsBalanceByPricingPlanAsync(
        Guid userId,
        PricingPlanType pricingPlanType,
        CancellationToken cancellationToken)
    {
        switch (pricingPlanType)
        {
            case PricingPlanType.Basic:
                await UserAICreditBalanceRepository.SetUserGpt3CreditsAsync(userId, OpenAICreditsCalculator.ConvertOpenAITokensToCredits(4_000_000), cancellationToken);
                break;
            case PricingPlanType.Standard:
                await UserAICreditBalanceRepository.SetUserGpt3CreditsAsync(userId, OpenAICreditsCalculator.ConvertOpenAITokensToCredits(4_000_000), cancellationToken);
                await UserAICreditBalanceRepository.SetUserGpt4CreditsAsync(userId, OpenAICreditsCalculator.ConvertOpenAITokensToCredits(500_000), cancellationToken);
                break;
            case PricingPlanType.Premium:
                await UserAICreditBalanceRepository.SetUserGpt3CreditsAsync(userId, OpenAICreditsCalculator.ConvertOpenAITokensToCredits(7_000_000), cancellationToken);
                await UserAICreditBalanceRepository.SetUserGpt4CreditsAsync(userId, OpenAICreditsCalculator.ConvertOpenAITokensToCredits(1_000_000), cancellationToken);
                break;
        }

        // Invalidate cache if exists
        var userGpt3CreditBalanceCacheKey = CacheKeyGenerator.GetUserGpt3CreditsBalanceKey(userId);
        await cacheService.Remove(userGpt3CreditBalanceCacheKey, cancellationToken: cancellationToken);

        var userGpt4CreditBalanceCacheKey = CacheKeyGenerator.GetUserGpt4CreditsBalanceKey(userId);
        await cacheService.Remove(userGpt4CreditBalanceCacheKey, cancellationToken: cancellationToken);
    }

    private async Task UpdateUserGpt3CreditBalanceCache(Guid userId, int credits, CancellationToken cancellationToken)
    {
        var userGpt3CreditBalanceCacheKey = CacheKeyGenerator.GetUserGpt3CreditsBalanceKey(userId);
        await cacheService.Set(userGpt3CreditBalanceCacheKey, credits, cancellationToken: cancellationToken);
    }

    private async Task UpdateUserGpt4CreditBalanceCache(Guid userId, int credits, CancellationToken cancellationToken)
    {
        var userGpt4CreditBalanceCacheKey = CacheKeyGenerator.GetUserGpt4CreditsBalanceKey(userId);
        await cacheService.Set(userGpt4CreditBalanceCacheKey, credits, cancellationToken: cancellationToken);
    }
}