using ZODs.Api.Repository.Dtos.AILens;
using ZODs.Api.Repository.QueryParams;
using ZODs.Api.Service.Dtos;
using ZODs.Api.Service.InputDtos;

namespace ZODs.Api.Service;

public interface IAILensService
{
    Task<ICollection<AILensInfoDto>> GetUserAILensesInfoAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get AI Lens instructions by lens id.
    /// </summary>
    /// <param name="lensId">AI Lens id.</param>
    /// <param name="tryGetFromCache">Flag indicating whether to try to get the instructions from cache.</param>
    /// <param name="cancellationToken">Instance of <see cref="CancellationToken"/>.</param>
    /// <returns>AILens instructions.</returns>
    Task<AILensInstructionsDto> GetAILensInstructionsAsync(Guid lensId, bool tryGetFromCache = true, CancellationToken cancellationToken = default);
    Task<ICollection<AILensInfoDto>> GetUserBuiltInLensesInfoAsync(
        Guid userId, 
        bool tryGetFromCache = true, 
        CancellationToken cancellationToken = default);
    Task SetUserAILensEnabledAsync(Guid lenseId, Guid userId, bool isEnabled, CancellationToken cancellationToken = default);
    Task<PagedResponse<UserAILensDto>> GatePagedUserAILensesAsync(GetPagedUserAILensesQuery query, Guid userId, CancellationToken cancellationToken);
    Task<PagedResponse<UserAILensDto>> GetPagedUserBuiltInAILensesAsync(GetUserBuiltInAILensesQuery query, Guid userId, CancellationToken cancellationToken);
    Task<AILensDto> CreateAILensAsync(AILensInputDto inputDto, Guid userId, CancellationToken cancellationToken);
    Task<AILensInputDto> UpdateAILensAsync(AILensInputDto inputDto, Guid userId, CancellationToken cancellationToken);
    Task<AILensDto> GetLensByIdAsync(Guid lensId, Guid userId, CancellationToken cancellationToken);
    Task<bool> AILensWithSameNameExists(CheckAILensNameExistsInputDto inputDto, Guid userId, CancellationToken cancellationToken);
}