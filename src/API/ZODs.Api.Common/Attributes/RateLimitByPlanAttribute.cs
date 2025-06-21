using ZODs.Api.Common.Enums;

namespace ZODs.Api.Common.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public sealed class RateLimitByPlanAttribute(
    RateLimitationType rateLimitationType) : Attribute
{
    public RateLimitationType RateLimitationType { get; } = rateLimitationType;
}
