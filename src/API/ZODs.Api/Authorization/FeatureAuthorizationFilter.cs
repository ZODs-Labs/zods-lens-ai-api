using ZODs.Api.Extensions;
using ZODs.Api.Service;
using ZODs.Api.Service.Common;
using ZODs.Api.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using ZODs.Api.Service.Factories.Interfaces;

namespace ZODs.Api.Authorization;

public class FeatureAuthorizationFilter(
    IFeatureLimitationStrategyFactory featureLimitationStrategyFactory,
    IUserInfoService userInfoService) : IAsyncAuthorizationFilter
{
    private readonly IFeatureLimitationStrategyFactory _featureLimitationStrategyFactory = featureLimitationStrategyFactory;
    private readonly IUserInfoService userInfoService = userInfoService;

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var attributes = context.ActionDescriptor.EndpointMetadata
            .OfType<ValidateFeatureAttribute>();

        foreach (var attribute in attributes)
        {
            var userId = context.HttpContext.User.GetUserId();
            var httpContext = context.HttpContext;
            var userPricingPlanType = httpContext.User.GetPricingPlanType();
            var requestedFeature = attribute.Feature;

            // Get user features
            var userFeatures = await userInfoService.GetUserFeaturesAsync(userId);
            var userHasFeature = userFeatures.Any(featureKey => featureKey == requestedFeature);

            if (!userHasFeature)
            {
                var message = attribute.ErrorMessage;
                if (string.IsNullOrWhiteSpace(message))
                {
                    message = "You don't have access to this feature.";
                }

                // User doesn't have access to this feature
                var response = new
                {
                    status = "failure",
                    reason = "feature_not_allowed",
                    message,
                };

                context.Result = new ObjectResult(response)
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                };

                return;
            }

            if (attribute.RequirePaidPlan)
            {
                var userInfo = await userInfoService.GetUserInfoCachedAsync(userId, httpContext.RequestAborted);

                if (!userInfo.IsPaidPlan)
                {
                    // User doesn't have paid plan
                    var response = new
                    {
                        status = "failure",
                        reason = "no_paid_plan",
                        message = "To access this feature you need to have paid plan. Please upgrade your plan.",
                    };
                    context.Result = new ObjectResult(response)
                    {
                        StatusCode = (int)HttpStatusCode.Forbidden
                    };

                    return;
                }
            }

            var pricingPlanTypes = attribute.PricingPlanTypes;
            if (pricingPlanTypes.Length > 0 && pricingPlanTypes.All(x => x.ToString() != userPricingPlanType))
            {
                // If user pricing plan is not specified in collection, then skip limitation check
                return;
            }

            // Check if user has reached requested feature limitations
            foreach (var limitationIndex in attribute.LimitationIndexes)
            {
                var strategy = _featureLimitationStrategyFactory.Create(limitationIndex);

                Guid? workspaceId = null;
                if (context.RouteData.Values.TryGetValue("workspaceId", out var workspaceIdObj) &&
                    Guid.TryParse(workspaceIdObj?.ToString(), out var parsedGuid))
                {
                    workspaceId = parsedGuid;
                }

                var limitationContext = FeatureLimitationContext.Create(userId, workspaceId);

                if (await strategy.HasReachedLimitationAsync(limitationContext))
                {
                    var message = FeatureLimitationMessages.GetLimitationMessage(limitationIndex);
                    if (string.IsNullOrWhiteSpace(message))
                    {
                        message = "You have reached defined limitation for this feature.";
                    }

                    var detailedResponse = new
                    {
                        status = "failure",
                        reason = "feature_limitation_reached",
                        limitation = limitationIndex.ToString(),
                        message,
                    };

                    context.Result = new ObjectResult(detailedResponse)
                    {
                        StatusCode = (int)HttpStatusCode.Forbidden
                    };

                    return;
                }
            }
        }
    }
}
