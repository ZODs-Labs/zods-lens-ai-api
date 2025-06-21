using ZODs.Api.Service.InputDtos.Snippet;

namespace ZODs.Api.Service.Validation.Interfaces
{
    public interface ISnippetValidationService
    {
        Task ValidateNonDuplicateSnippet(string name, string trigger, Guid userId, Guid? snippetId, Guid? workspaceId, bool isWorkspaceOwned, CancellationToken cancellationToken);
        Task ValidateSnippetDto(UpsertSnippetInputDto dto, Guid userId, CancellationToken cancellationToken);
        Task ValidateSnippetTriggerPrefixExists(Guid snippetTriggerPrefixId, Guid? userId, Guid? workspaceId, CancellationToken cancellationToken);
        Task ValidateUserHasWritePermissionsForSnippet(Guid snippetId, Guid userId, CancellationToken cancellationToken);
    }
}