using Microsoft.Extensions.Caching.Distributed;
using ZODs.Api.Common.Attributes;
using ZODs.Api.Common.Constants;
using ZODs.Api.Common.Helpers;
using ZODs.Api.Common.Models;
using ZODs.Api.ExceptionHandling;
using ZODs.Api.Extensions;
using ZODs.Api.Service;
using System.Text;
using System.Text.Json;

namespace ZODs.Api.Middlewares;

public sealed class RateLimitingMiddleware(
    RequestDelegate next,
    IDistributedCache cache,
    ILogger<RateLimitingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly IDistributedCache _cache = cache;
    private readonly ILogger<RateLimitingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var rateLimitInfo = endpoint?.Metadata.GetMetadata<RateLimitByPlanAttribute>();

        if (rateLimitInfo != null)
        {
            try
            {
                var userInfoService = context.RequestServices.GetRequiredService<IUserInfoService>();
                var userId = context.User.GetUserId();
                var userPricingPlan = await userInfoService.GetUserPricingPlanAsync(userId, context.RequestAborted);

                var cacheKey = CacheKeyGenerator.GetUserRateLimitationKey(rateLimitInfo.RateLimitationType, userId);
                var rateLimitationValue = await _cache.GetStringAsync(cacheKey);

                var (Minutes, MaxRequests) = PricingPlanRateLimitationResolver.GetRateLimitationInfo(userPricingPlan.PricingPlanType, rateLimitInfo.RateLimitationType);

                if (rateLimitationValue != null)
                {
                    var rateLimitation = JsonSerializer.Deserialize<RateLimitationInfo>(rateLimitationValue);
                    if (rateLimitation.RequestsCount >= MaxRequests)
                    {
                        var message = PricingPlanRateLimitationResolver.GetRateLimitationMessage(rateLimitInfo.RateLimitationType);
                        var formattedMessage = FormatCapMessage(message, rateLimitation.ExpirationDate);

                        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                        var errorDetails = new ErrorDetails
                        {
                            Message = formattedMessage,
                            StatusCode = StatusCodes.Status429TooManyRequests,
                        };

                        await context.Response.WriteAsync(errorDetails.ToString());
                        return;
                    }

                    rateLimitation.RequestsCount++;
                    await UpdateRateLimitationInfoInCache(cacheKey, rateLimitation);
                }
                else
                {
                    await AddRateLimitationInfoToCache(cacheKey, Minutes);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Error occurred while processing rate limitation.");
                throw;
            }
        }

        await _next(context);
    }

    private static string FormatCapMessage(string message, DateTime capExpirationDateTime)
    {
        var expirationTimeSpan = capExpirationDateTime - DateTime.UtcNow;
        var tryAgaInStringBuilder = new StringBuilder();

        if (expirationTimeSpan.Minutes > 0)
        {
            tryAgaInStringBuilder.Append($"{expirationTimeSpan.Minutes} minute(s)");
        }

        if (expirationTimeSpan.Seconds > 0)
        {
            tryAgaInStringBuilder.Append($" and {expirationTimeSpan.Seconds} second(s)");
        }

        return string.Format(message, tryAgaInStringBuilder);
    }

    private async Task AddRateLimitationInfoToCache(
        string cacheKey,
        int timeWindowInMinutes,
        CancellationToken cancellationToken = default)
    {
        var expirationDate = DateTime.UtcNow.AddMinutes(timeWindowInMinutes);
        var newRteLimitation = new RateLimitationInfo(1, expirationDate);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(timeWindowInMinutes),
        };

        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(newRteLimitation), options, cancellationToken);
    }

    private async Task UpdateRateLimitationInfoInCache(
        string cacheKey,
        RateLimitationInfo rateLimitation,
        CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = rateLimitation.ExpirationDate - DateTime.UtcNow,
        };

        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(rateLimitation), options, cancellationToken);
    }
}
