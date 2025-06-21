using ZODs.Api.Repository.Entities.Entities;
using System.ComponentModel.DataAnnotations;

namespace ZODs.Api.Repository.Entities;

public sealed class Workspace : BaseEntity
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
    public bool IsActive { get; set; }

    public ICollection<WorkspaceMember> Members { get; set; } = new List<WorkspaceMember>();

    public ICollection<WorkspaceSnippet> Snippets { get; set; } = new List<WorkspaceSnippet>();

    public ICollection<AILens> AILenses { get; set; } = new List<AILens>();

    public ICollection<WorkspaceMemberInvite> Invites { get; set; } = new List<WorkspaceMemberInvite>();

    public ICollection<SnippetTriggerPrefix> TriggerPrefixes { get; set; } = new List<SnippetTriggerPrefix>();
}
