namespace ZODs.Api.Common.Models;

public sealed class RateLimitationInfo
{
    public RateLimitationInfo(int requestsCount, DateTime expirationDate)
    {
        RequestsCount = requestsCount;
        ExpirationDate = expirationDate;
    }

    public int RequestsCount { get; set; }
    public DateTime ExpirationDate { get; set; }
}
