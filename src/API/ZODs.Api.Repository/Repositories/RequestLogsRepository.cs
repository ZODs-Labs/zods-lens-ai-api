using ZODs.Api.Repository.Entities;

namespace ZODs.Api.Repository;

public sealed class RequestLogsRepository(ZodsContext context) : Repository<RequestLog, ZodsContext>(context), IRequestLogsRepository
{
}
