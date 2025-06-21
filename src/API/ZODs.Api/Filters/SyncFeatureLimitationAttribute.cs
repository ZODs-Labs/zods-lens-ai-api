using ZODs.Api.Extensions;
using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Service.Managers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ZODs.Api.Filters
{
    public sealed class SyncFeatureLimitationAttribute : ActionFilterAttribute
    {
        private readonly FeatureLimitationIndex[] _featureLimitationIndexes;

        public SyncFeatureLimitationAttribute(
            params FeatureLimitationIndex[] featureLimitationIndexes)
        {
            _featureLimitationIndexes = featureLimitationIndexes;
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var userId = context.HttpContext.User.GetUserId();

            var featureLimitationSyncManager = context.HttpContext.RequestServices.GetRequiredService<IFeatureLimitationSyncManager>();
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<SyncFeatureLimitationAttribute>>();

            try
            {
                // If result is ObjectResult and status code indicates success
                if (context.Result is ObjectResult result &&
                    result.StatusCode.HasValue && 
                    (int)result.StatusCode >= 200 && 
                    (int)result.StatusCode <= 299)
                {
                    foreach (var featureLimitationIndex in _featureLimitationIndexes)
                    {
                        featureLimitationSyncManager.QueueLimitationUsageSync(userId, featureLimitationIndex);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to sync feature limitation for user with id {userId}.", userId);
            }
            finally
            {
                base.OnActionExecuted(context);
            }
        }
    }
}