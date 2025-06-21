using ZODs.Api.Repository.Dtos;
using ZODs.Api.Repository.Entities.Enums;

namespace ZODs.Api.Service;

public interface IUserAICreditBalanceService
{
    Task AddUserGpt3CreditsAsync(Guid userId, int credits, CancellationToken cancellationToken);
    Task AddUserGpt4CreditsAsync(Guid userId, int credits, CancellationToken cancellationToken);
    Task<UserAICreditsBalanceDto> GetUserAICreditsBalanceAsync(Guid userId, CancellationToken cancellationToken);
    Task<int> GetUserGpt3CreditBalanceAsync(Guid userId, CancellationToken cancellationToken);
    Task<int> GetUserGpt4CreditBalanceAsync(Guid userId, CancellationToken cancellationToken);
    Task SetUserAICreditsBalanceByPricingPlanAsync(Guid userId, PricingPlanType pricingPlanType, CancellationToken cancellationToken);
    Task UseUserGpt3CreditsAsync(Guid userId, int credits, CancellationToken cancellationToken);
    Task UseUserGpt4CreditsAsync(Guid userId, int credits, CancellationToken cancellationToken);
}