using ZODs.Api.Common.Enums;
using ZODs.Api.Repository;
using ZODs.Api.Repository.Interfaces;
using ZODs.Api.Repository.Repositories.Interfaces;
using ZODs.Common.Constants;

namespace ZODs.Api.Service.Services;

public sealed class UserSubscriptionService : IUserSubscriptionService
{
    private readonly IUnitOfWork<ZodsContext> _unitOfWork;

    public UserSubscriptionService(IUnitOfWork<ZodsContext> unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    private IUsersRepository UsersRepository => _unitOfWork.GetRepository<IUsersRepository>();
    private IWorkspacesRepository WorkspacesRepository => _unitOfWork.GetRepository<IWorkspacesRepository>();
    private IPricingPlanRepository PricingPlanRepository => _unitOfWork.GetRepository<IPricingPlanRepository>();

    public async Task HandleUserSubscriptionStatusUpdateAsync(
        string subscriptionStatus,
        Guid userId,
        CancellationToken cancellationToken)
    {

    }
}