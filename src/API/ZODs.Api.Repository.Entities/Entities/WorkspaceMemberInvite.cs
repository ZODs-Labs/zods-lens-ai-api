using ZODs.Api.Repository.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace ZODs.Api.Repository.Entities;

public sealed class WorkspaceMemberInvite : BaseEntity
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public WorkspaceMemberRoleIndex RoleIndex { get; set; }

    [Required]
    public DateTime ExpiresAt { get; set; }

    [Required]
    public bool IsUsed { get; set; }

    public DateTime? AcceptedAt { get; set; }

    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;

    public Guid? InvitedByUserId { get; set; }
    public User? InvitedByUser { get; set; }

    public Guid? AcceptedByUserId { get; set; }
    public User? AcceptedByUser { get; set; }
}