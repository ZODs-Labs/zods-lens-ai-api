using ZODs.Api.Repository.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace ZODs.Api.Repository.Entities.Entities
{
    public sealed class AILens : BaseEntity
    {
        [Required]
        [MinLength(1)]
        [MaxLength(20)]
        public string Name { get; set; } = null!;

        [Required]
        [MinLength(1)]
        [MaxLength(1000)]
        public string BehaviorInstruction { get; set; } = null!;

        [Required]
        [MinLength(1)]
        [MaxLength(1000)]
        public string ResponseInstruction { get; set; } = null!;

        [MaxLength(100)]
        public string? Tooltip { get; set; }

        [Required]
        public AILensTargetKind TargetKind { get; set; }

        [Required]
        public bool IsEnabled { get; set; }

        [Required]
        public bool IsBuiltIn { get; set; }

        // Optional user owner
        public Guid? UserId { get; set; }
        public User? User { get; set; } = null!;

        // Optional workspace owner
        public Guid? WorkspaceId { get; set; }
        public Workspace? Workspace { get; set; } = null!;
    }
}