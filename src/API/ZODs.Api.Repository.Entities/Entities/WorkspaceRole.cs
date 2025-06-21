using ZODs.Api.Repository.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace ZODs.Api.Repository.Entities
{
    public sealed class WorkspaceRole : BaseEntity
    {
        [Required]
        [MinLength(2)]
        [MaxLength(50)]
        public string Name { get; set; } = null!;

        [Required]
        [MinLength(2)]
        [MaxLength(500)]
        public string Description { get; set; } = null!;

        [Required]
        public WorkspaceMemberRoleIndex Index { get; set; }

        public ICollection<WorkspaceMemberRole> Members { get; set; } = new List<WorkspaceMemberRole>();
    }
}