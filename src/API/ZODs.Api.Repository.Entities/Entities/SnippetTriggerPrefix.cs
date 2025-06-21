using ZODs.Api.Repository.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace ZODs.Api.Repository.Entities
{
    public sealed class SnippetTriggerPrefix : BaseEntity
    {
        [Required]
        [MinLength(1)]
        [MaxLength(20)]
        public string Prefix { get; set; } = null!;

        [Required]
        public SnippetTriggerPrefixType Type { get; set; }

        public Guid? UserId { get; set; }
        public User? User { get; set; } = null!;

        public Guid? WorkspaceId { get; set; }
        public Workspace? Workspace { get; set; }

        public ICollection<Snippet> Snippets { get; set; } = new List<Snippet>();
    }
}