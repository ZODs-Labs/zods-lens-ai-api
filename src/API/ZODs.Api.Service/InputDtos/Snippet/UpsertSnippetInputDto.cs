using ZODs.Common.Attributes;
using System.ComponentModel.DataAnnotations;

namespace ZODs.Api.Service.InputDtos.Snippet
{
    public sealed class UpsertSnippetInputDto
    {
        public Guid? Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(500)]
        public string Description { get; set; } = null!;

        [Required]
        [MinLength(3)]
        public string CodeSnippet { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        [MinLength(2)]
        public string BaseTrigger { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Language { get; set; } = null!;

        [Required]
        public bool IsWorkspaceOwned { get; set; }

        [NotEmptyGuid]
        public Guid? WorkspaceId { get; set; }

        [NotEmptyGuid]
        public Guid? TriggerPrefixId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}