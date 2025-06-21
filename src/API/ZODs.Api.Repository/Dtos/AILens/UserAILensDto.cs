using ZODs.Api.Repository.Entities.Enums;

namespace ZODs.Api.Repository.Dtos.AILens
{
    public sealed class UserAILensDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public string? Tooltip { get; set; }

        public AILensTargetKind TargetKind { get; set; }

        public bool IsEnabled { get; set; }

        public bool IsBuiltIn { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}