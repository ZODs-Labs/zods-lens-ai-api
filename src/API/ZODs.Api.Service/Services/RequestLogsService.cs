using ZODs.Api.Repository;
using ZODs.Api.Repository.Entities;
using ZODs.Api.Repository.Interfaces;
using ZODs.Common.Extensions;

namespace ZODs.Api.Service;

public sealed class RequestLogsService(IUnitOfWork<ZodsContext> unitOfWork) : IRequestLogsService
{
    private readonly IUnitOfWork<ZodsContext> unitOfWork = unitOfWork;
    private IRequestLogsRepository RequestLogsRepository => this.unitOfWork.GetRepository<IRequestLogsRepository>();

    public async Task LogRequestAsync(
        string route, 
        string metadata, 
        Guid? userId,
        CancellationToken cancellationToken)
    {
        var requestLog = new RequestLog
        {
            Route = route,
            Metadata = metadata,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
        };

        await this.RequestLogsRepository.Insert(requestLog, cancellationToken).NoSync();
        await unitOfWork.CommitAsync(cancellationToken).NoSync();
    }
}
