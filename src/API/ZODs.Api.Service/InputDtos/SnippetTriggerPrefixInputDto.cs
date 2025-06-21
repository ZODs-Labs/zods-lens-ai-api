using System.ComponentModel.DataAnnotations;

namespace ZODs.Api.Service.InputDtos
{
    public sealed class SnippetTriggerPrefixInputDto
    {
        public Guid? Id { get; set; }

        [Required]
        [MinLength(1)]
        [MaxLength(3)]
        public string Prefix { get; set; } = null!;

        public Guid? UserId { get; set; }

        public Guid? WorkspaceId { get; set; }
    }
}