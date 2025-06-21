using ZODs.Api.Repository;
using ZODs.Api.Repository.Interfaces;
using ZODs.Api.Service.Validation.Interfaces;

namespace ZODs.Api.Service.Validation;

public sealed class AILensValidationService : IAILensValidationService
{
    private readonly IUnitOfWork<ZodsContext> unitOfWork;

    public AILensValidationService(IUnitOfWork<ZodsContext> unitOfWork)
    {
        this.unitOfWork = unitOfWork;
    }

    private IAILensRepository AILensRepostiory => unitOfWork.GetRepository<IAILensRepository>();
}