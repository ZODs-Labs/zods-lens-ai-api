using ZODs.Api.Service.Interfaces;

namespace ZODs.Api.Service.Common
{
    public sealed class FeatureLimitationContext : IFeatureLimitationContext
    {
        public Guid UserId { get; set; }

        public Guid? WorkspaceId { get; set; }

        public static FeatureLimitationContext Create(Guid userId)
             => new()
             { UserId = userId };

        public static FeatureLimitationContext Create(Guid userId, Guid? workspaceId)
            => new()
            { UserId = userId, WorkspaceId = workspaceId };
    }
}