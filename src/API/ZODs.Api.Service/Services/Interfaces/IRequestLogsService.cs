
namespace ZODs.Api.Service;

public interface IRequestLogsService
{
    Task LogRequestAsync(
        string route,
        string metadata,
        Guid? userId,
        CancellationToken cancellationToken);
}
