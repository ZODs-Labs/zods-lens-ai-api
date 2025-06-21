using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ZODs.Api.Common.Attributes;
using ZODs.Api.Extensions;
using System.Net;

namespace ZODs.Api.Authorization;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class ValidSubscriptionAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (context == null)
        {
            return;
        }

        var endpointMetadata = context.ActionDescriptor.EndpointMetadata;

        var hasNoSubscriptionOrAnonymousAttribute = endpointMetadata.Any(metadata => metadata is AllowNoSubscriptionAttribute ||
                                                                                     metadata is AllowAnonymousAttribute);
        if (hasNoSubscriptionOrAnonymousAttribute)
        {
            // This endpoint doesn't require a valid subscription.
            return;
        }

        var user = context.HttpContext?.User;
        if (user != null)
        {
            var hasValidSubscription = user.HasValidSubscriptionFlag();
            if (!hasValidSubscription)
            {
                // User doesn't have access to this feature
                var response = new
                {
                    status = "failure",
                    reason = "no_valid_subscription",
                    message = "You don't have a valid subscription. Please purchase a plan to use this feature.",
                };

                context.Result = new ObjectResult(response)
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                };

                return;
            }
        }

        var paidPlanOnly = endpointMetadata.Any(metadata => metadata is PaidPlanOnly);
        if (paidPlanOnly)
        {
            var hasPaidPlan = user.IsPaidPricingPlan();
            if (!hasPaidPlan)
            {
                var path = context.HttpContext.Request.Path.Value;
                var message = "To use this feature, you need to have a paid plan.";

                if (path.StartsWith("/api/gpt", StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith("/api/ai", StringComparison.OrdinalIgnoreCase))
                {
                    message = "To use this feature, you need to have a paid plan. Please [upgrade your plan](https://app.zods.pro/plan).";
                }

                // User does not have a paid plan
                var response = new
                {
                    status = "failure",
                    reason = "no_paid_plan",
                    message,
                };

                context.Result = new ObjectResult(response)
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                };
            }
        }
    }
}
