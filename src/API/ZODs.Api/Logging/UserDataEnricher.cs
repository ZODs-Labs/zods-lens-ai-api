using Serilog.Core;
using Serilog.Events;
using ZODs.Api.Extensions;

namespace ZODs.Api.Logging;

public class UserDataEnricher(IHttpContextAccessor httpContextAccessor) : ILogEventEnricher
{
    private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor;

    public UserDataEnricher() : this(new HttpContextAccessor())
    {
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var userId = httpContextAccessor.HttpContext?.User?.GetUserIdOptional();
        if (userId != null)
        {
            logEvent.AddPropertyIfAbsent(
                 propertyFactory.CreateProperty("UserId", userId));
        }
    }
}
