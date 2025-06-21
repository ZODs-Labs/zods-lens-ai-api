using ZODs.Api.Repository;
using ZODs.Api.Repository.Interfaces;
using ZODs.Api.Repository.Repositories.Interfaces;

namespace ZODs.Api.Service.Strategies.UserSubscriptionTransition;

public abstract class BaseUserSubscriptionTransitionStrategy 
{
    protected readonly IUnitOfWork<ZodsContext> unitOfWork;

    protected BaseUserSubscriptionTransitionStrategy(IUnitOfWork<ZodsContext> unitOfWork)
    {
        this.unitOfWork = unitOfWork;
    }

    protected IPricingPlanRepository PricingPlanRepository => unitOfWork.GetRepository<IPricingPlanRepository>();
}