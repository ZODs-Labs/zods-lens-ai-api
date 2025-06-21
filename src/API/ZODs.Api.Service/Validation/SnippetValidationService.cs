using ZODs.Common.Exceptions;
using ZODs.Api.Repository;
using ZODs.Api.Service.InputDtos.Snippet;
using ZODs.Api.Service.Validation.Interfaces;
using ZODs.Api.Repository.Interfaces;
using ZODs.Common.Extensions;

namespace ZODs.Api.Service.Validation
{
    public sealed class SnippetValidationService : ISnippetValidationService
    {
        private readonly IUnitOfWork<ZodsContext> unitOfWork;

        public SnippetValidationService(IUnitOfWork<ZodsContext> unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        private ISnippetsRepository SnippetsRepository => unitOfWork.GetRepository<ISnippetsRepository>();
        private ISnippetTriggerPrefixesRepository SnippetTriggerPrefixesRepository => unitOfWork.GetRepository<ISnippetTriggerPrefixesRepository>();

        public async Task ValidateSnippetDto(
            UpsertSnippetInputDto dto,
            Guid userId,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dto);
            if (dto.Id.HasValue)
            {
                await ValidateUserHasWritePermissionsForSnippet(dto.Id.Value, userId, cancellationToken).NoSync();
            }

            await this.ValidateNonDuplicateSnippet(
                dto.Name,
                dto.BaseTrigger,
                userId,
                dto.Id,
                dto.WorkspaceId,
                dto.IsWorkspaceOwned,
                cancellationToken).NoSync();

            if (dto.IsWorkspaceOwned && !dto.WorkspaceId.HasValue)
            {
                throw new BusinessValidationException("Owner workspace id is required.");
            }

            if (dto.TriggerPrefixId.HasValue)
            {
                var workspaceId = dto.IsWorkspaceOwned ? dto.WorkspaceId : null;
                Guid? snippetUserId = dto.IsWorkspaceOwned ? null : userId;

                await ValidateSnippetTriggerPrefixExists(dto.TriggerPrefixId.Value, snippetUserId, workspaceId, cancellationToken).NoSync();
            }
        }

        public async Task ValidateNonDuplicateSnippet(
            string name,
            string trigger,
            Guid userId,
            Guid? snippetId,
            Guid? workspaceId,
            bool isWorkspaceOwned,
            CancellationToken cancellationToken)
        {
            var duplicateSnippetExists = await SnippetsRepository.DuplicateSnippetExists(
                name,
                trigger,
                snippetId,
                workspaceId,
                userId,
                isWorkspaceOwned,
                cancellationToken).NoSync();

            if (duplicateSnippetExists)
            {
                throw new DuplicateEntityException("Snippet with the same name or trigger already exists.");
            }
        }

        public async Task ValidateUserHasWritePermissionsForSnippet(
            Guid snippetId,
            Guid userId,
            CancellationToken cancellationToken)
        {
            var hasWritePermissionForSnippet = await SnippetsRepository.HasUserSnippetWritePermission(
                snippetId,
                userId,
                cancellationToken).NoSync();

            if (!hasWritePermissionForSnippet)
            {
                throw new UnauthorizedAccessException($"User {userId} does not have write permissions for snippet {snippetId}");
            }
        }

        public async Task ValidateSnippetTriggerPrefixExists(
            Guid snippetTriggerPrefixId,
            Guid? userId,
            Guid? workspaceId,
            CancellationToken cancellationToken)
        {
            var prefixExists = await this.SnippetTriggerPrefixesRepository.ExistsAsync(
                        x => x.Id == snippetTriggerPrefixId &&
                        (userId != null && x.UserId == userId ||
                         workspaceId != null && x.WorkspaceId == workspaceId),
                        cancellationToken: cancellationToken).NoSync();

            if (!prefixExists)
            {
                throw new BusinessValidationException($"Snippet trigger prefix with id {snippetTriggerPrefixId} does not exist.");
            }
        }
    }
}