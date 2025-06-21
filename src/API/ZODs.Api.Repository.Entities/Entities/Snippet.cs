using System.ComponentModel.DataAnnotations;

namespace ZODs.Api.Repository.Entities;

public sealed class Snippet : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!; 

    [MaxLength(500)]
    public string? Description { get; set; } = null!;

    [Required]
    [MinLength(3)]
    public string CodeSnippet { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    [MinLength(2)]
    public string BaseTrigger { get; set; } = null!;

    [Required]
    public bool IsWorkspaceOwned { get; set; }

    [Required]
    [MaxLength(50)]
    public string Language { get; set; } = null!;

    public string Trigger => TriggerPrefix != null ? $"{TriggerPrefix.Prefix}.{BaseTrigger}" : $"zods.{BaseTrigger}";

    // Optional user owner
    public Guid? UserId { get; set; }
    public User? User { get; set; } = null!;

    // Optional workspace owner
    public Guid? WorkspaceId { get; set; }
    public Workspace? Workspace { get; set; } = null!;

    // Optional trigger prefix
    public Guid? TriggerPrefixId { get; set; }
    public SnippetTriggerPrefix? TriggerPrefix { get; set; } = null!;

    public ICollection<WorkspaceSnippet> SharedWorkspaces { get; set; } = new List<WorkspaceSnippet>();
}
