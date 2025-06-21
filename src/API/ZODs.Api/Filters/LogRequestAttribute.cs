using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing.Patterns;
using ZODs.Api.Common.Interfaces;
using ZODs.Api.Extensions;
using ZODs.Api.Service;

namespace ZODs.Api.Filters;

public sealed class LogRequestAttribute : ActionFilterAttribute
{
    public LogRequestAttribute()
    {
    }

    public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        var httpContext = context.HttpContext;
        var endpoint = httpContext.GetEndpoint();
        var userId = httpContext.User.GetUserId();
        var requestPath = httpContext.Request.Path.Value;

        httpContext.Items.TryGetValue("RequestLogMetadata", out var requestLogMetadata);

        var backgroundTaskQueue = httpContext.RequestServices.GetRequiredService<IBackgroundTaskQueue>();
        backgroundTaskQueue.QueueBackgroundWorkItem(async (cancellationToken, serviceProvider) =>
        {
            var scope = serviceProvider.CreateScope();
            var requestLogsService = scope.ServiceProvider.GetRequiredService<IRequestLogsService>();
            await requestLogsService.LogRequestAsync(requestPath, requestLogMetadata?.ToString(), userId, cancellationToken);
        });

        await next();
    }
}
