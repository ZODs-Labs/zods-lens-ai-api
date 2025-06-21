using ZODs.Api.Repository.Entities.AI;
using ZODs.Api.Repository.Interfaces;

namespace ZODs.Api.Repository;

public interface IUserAICreditBalanceRepository : IRepository<UserAICreditBalance>
{
    Task AddUserGpt3CreditsAsync(Guid userId, int credits, CancellationToken cancellationToken);
    Task AddUserGpt4CreditsAsync(Guid userId, int credits, CancellationToken cancellationToken);
    Task SetUserGpt3CreditsAsync(Guid userId, int credits, CancellationToken cancellationToken);
    Task SetUserGpt4CreditsAsync(Guid userId, int credits, CancellationToken cancellationToken);
    Task<int> UseUserGpt3CreditsAsync(Guid userId, int credits, CancellationToken cancellationToken);
    Task<int> UseUserGpt4CreditsAsync(Guid userId, int credits, CancellationToken cancellationToken);
}
