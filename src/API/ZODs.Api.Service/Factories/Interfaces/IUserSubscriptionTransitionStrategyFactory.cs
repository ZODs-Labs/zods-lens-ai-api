using ZODs.Api.Common.Enums;
using ZODs.Api.Service.Strategies.UserSubscriptionTransition.Interfaces;

namespace ZODs.Api.Service.Factories.Interfaces;

public interface IUserSubscriptionTransitionStrategyFactory
{
    IUserSubscriptionTransitionStrategy Create(UserSubscriptionTransition subscriptionTransition);
}