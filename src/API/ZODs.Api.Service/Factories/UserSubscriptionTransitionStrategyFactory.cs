using Microsoft.Extensions.DependencyInjection;
using ZODs.Api.Common.Enums;
using ZODs.Api.Service.Factories.Interfaces;
using ZODs.Api.Service.Strategies.UserSubscriptionTransition;
using ZODs.Api.Service.Strategies.UserSubscriptionTransition.Interfaces;

namespace ZODs.Api.Service.Factories;

public sealed class UserSubscriptionTransitionStrategyFactory : IUserSubscriptionTransitionStrategyFactory
{
    private readonly IServiceProvider _serviceProvider;

    public UserSubscriptionTransitionStrategyFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IUserSubscriptionTransitionStrategy Create(UserSubscriptionTransition subscriptionTransition)
    {
        return subscriptionTransition switch
        {
            UserSubscriptionTransition.ExpiredToActive => _serviceProvider.GetRequiredService<ExpiredToActiveSubscriptionTransitionStrategy>(),
            UserSubscriptionTransition.ToPastDue => _serviceProvider.GetRequiredService<PastDueSubscriptionTransitionStrategy>(),
            UserSubscriptionTransition.ToUnpaid => _serviceProvider.GetRequiredService<UnpaidSubscriptionTransitionStrategy>(),
            UserSubscriptionTransition.ToCancelled => _serviceProvider.GetRequiredService<CancelledSubscriptionTransitionStrategy>(),
            UserSubscriptionTransition.ToExpired => _serviceProvider.GetRequiredService<ExpiredSubscriptionTransitionStrategy>(),
            _ => throw new NotImplementedException($"User subscription transition strategy for subscription {subscriptionTransition} is not implemented")
        };
    }
}