using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Service.Dtos;

namespace ZODs.Api.Service.Validation.Interfaces
{
    public interface IWorkspaceValidationService
    {
        Task ValidateMemberInvitationDoesNotExists(Guid workspaceId, string email, CancellationToken cancellationToken);
        Task ValidateMemberToAddRoleIsLowerThanExecutingUser(Guid workspaceId, Guid userId, WorkspaceMemberRoleIndex roleIndex, CancellationToken cancellationToken);
        Task<bool> ValidateUserHasValidWorkspaceInvite(Guid userId, Guid inviteId, CancellationToken cancellationToken);
        Task ValidateUserIsWorkspaceMember(Guid workspaceId, Guid userId, CancellationToken cancellationToken);
        Task ValidateUserIsWorkspaceOwner(Guid workspaceId, Guid userId, CancellationToken cancellationToken);
        Task ValidateWorkspaceDtoForUpsert(WorkspaceDto workspaceDto, Guid userId, CancellationToken cancellationToken);
        Task ValidateWorkspaceExists(Guid workspaceId, CancellationToken cancellationToken);
    }
}