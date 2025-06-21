using ZODs.Api.Repository.Common;
using ZODs.Api.Repository.Dtos.AILens;
using ZODs.Api.Repository.Entities.Entities;
using ZODs.Api.Repository.Interfaces;
using ZODs.Api.Repository.QueryParams;

namespace ZODs.Api.Repository;

public interface IAILensRepository: IRepository<AILens>
{
    Task<ICollection<AILensInfoDto>> GetAILensesInfoAsync(Guid userId,
        bool includeBuiltIn = true, CancellationToken cancellationToken = default);
    Task<AILensInstructionsDto?> GetAILensInstructionsAsync(Guid lensId, CancellationToken cancellationToken = default);
    Task<ICollection<AILensInfoDto>> GetBuiltInLensesInfoAsync(CancellationToken cancellationToken = default);
    Task<PagedEntities<UserAILensDto>> GetPagedUserAILensesAsync(GetPagedUserAILensesQuery query, Guid userId, CancellationToken cancellationToken = default);
    Task<PagedEntities<UserAILensDto>> GetPagedUserBuiltInAILensesAsync(GetUserBuiltInAILensesQuery query, Guid userId, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, bool>> GetUserBuiltInAILensesEnabledAsync(ICollection<Guid> builtInAILenseIds, Guid userId, CancellationToken cancellationToken = default);
    Task<int> GetUserOwnAILensesCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task SetUserAILensEnabledAsync(Guid lenseId, Guid userId, bool isEnabled, CancellationToken cancellationToken = default);
}