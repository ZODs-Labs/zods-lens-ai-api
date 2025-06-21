using ZODs.Common.Exceptions;
using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Repository.Entities;
using ZODs.Api.Repository;
using ZODs.Api.Service.Dtos;
using ZODs.Api.Service.Validation.Interfaces;
using ZODs.Api.Repository.Interfaces;
using ZODs.Api.Common.Extensions;
using ZODs.Common.Extensions;

namespace ZODs.Api.Service.Validation
{
    public sealed class WorkspaceValidationService : IWorkspaceValidationService
    {
        private readonly IUnitOfWork<ZodsContext> unitOfWork;

        public WorkspaceValidationService(IUnitOfWork<ZodsContext> unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        private IUsersRepository UsersRepository => unitOfWork.GetRepository<IUsersRepository>();
        private IWorkspacesRepository WorkspacesRepository => unitOfWork.GetRepository<IWorkspacesRepository>();
        private IWorkspaceMemberInvitesRepository WorkspaceMemberInvitesRepository => unitOfWork.GetRepository<IWorkspaceMemberInvitesRepository>();

        public async Task ValidateWorkspaceDtoForUpsert(
          WorkspaceDto workspaceDto,
          Guid userId,
          CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(workspaceDto);

            var sameNameWorkspaceExists = await this.WorkspacesRepository.UserScopedWorkspaceWithSameNameExistsAsync(
                workspaceDto.Name,
                userId,
                workspaceDto.Id,
                cancellationToken);

            if (sameNameWorkspaceExists)
            {
                throw new BusinessValidationException("Workspace with the same name already exists.");
            }
        }

        public async Task ValidateUserIsWorkspaceOwner(
            Guid workspaceId,
            Guid userId,
            CancellationToken cancellationToken)
        {
            var userIsOwner = await this.WorkspacesRepository.IsUserWorkspaceOwnerAsync(
                userId, workspaceId, cancellationToken);

            if (!userIsOwner)
            {
                throw new UnauthorizedAccessException("User is not the owner of the workspace.");
            }
        }

        public async Task ValidateWorkspaceExists(Guid workspaceId, CancellationToken cancellationToken)
        {
            var workspaceExists = await this.WorkspacesRepository.ExistsAsync(w => w.Id == workspaceId, cancellationToken: cancellationToken);
            if (!workspaceExists)
            {
                throw new KeyNotFoundException(typeof(Workspace).NotFoundValidationMessage(workspaceId));
            }
        }

        public async Task ValidateUserIsWorkspaceMember(Guid workspaceId, Guid userId, CancellationToken cancellationToken)
        {
            var userIsMember = await this.WorkspacesRepository.IsUserWorkspaceMemberAsync(userId, workspaceId, cancellationToken).NoSync();
            if (!userIsMember)
            {
                throw new UnauthorizedAccessException("User is not a member of the workspace.");
            }
        }

        public async Task ValidateMemberToAddRoleIsLowerThanExecutingUser(
            Guid workspaceId,
            Guid userId,
            WorkspaceMemberRoleIndex roleIndex,
            CancellationToken cancellationToken)
        {
            var isLowerRoleThanExecutingUserRole = await WorkspacesRepository.IsWorkspaceRoleLowerThanUserWorkspaceRole(
                workspaceId,
                userId,
                roleIndex,
                cancellationToken).NoSync();

            if (!isLowerRoleThanExecutingUserRole)
            {
                throw new BusinessValidationException("User cannot add a member with a role higher or equals to his own.");
            }
        }

        public async Task ValidateMemberInvitationDoesNotExists(
            Guid workspaceId,
            string email,
            CancellationToken cancellationToken)
        {
            var memberInvitationExists = await this.WorkspaceMemberInvitesRepository.ExistsAsync(
                      x => x.Email == email &&
                           x.WorkspaceId == workspaceId &&
                           !x.IsUsed,
                      cancellationToken: cancellationToken)
                .NoSync();

            if (memberInvitationExists)
            {
                throw new BusinessValidationException("Member already invited.");
            }
        }

        public async Task<bool> ValidateUserHasValidWorkspaceInvite(
            Guid userId,
            Guid inviteId,
            CancellationToken cancellationToken)
        {
            var email = await this.UsersRepository.FirstOrDefaultAsync(
                           u => u.Id == userId,
                           u => u.Email,
                           cancellationToken: cancellationToken)
                .NoSync();

            var hasInvite = await this.WorkspaceMemberInvitesRepository.ExistsAsync(
                  x => x.Id == inviteId &&
                       x.Email == email &&
                       !x.IsUsed &&
                       x.ExpiresAt > DateTime.UtcNow,
                  cancellationToken: cancellationToken).NoSync();

            return hasInvite;
        }
    }
}