using System.ComponentModel.DataAnnotations;

namespace ZODs.Api.Repository.Entities;

public sealed class WorkspaceSnippetsVersion : BaseEntity
{
    [Required]
    public int SnippetsVersion { get; set; }

    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;
}