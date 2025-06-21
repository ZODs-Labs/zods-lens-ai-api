using ZODs.Api.Repository.Dtos;
using ZODs.Api.Repository.QueryParams;
using ZODs.Api.Service.Dtos;
using ZODs.Api.Service.InputDtos;

namespace ZODs.Api.Service
{
    public interface ISnippetTriggerPrefixService
    {
        Task<SnippetTriggerPrefixDto> CreateSnippetTriggerPrefixAsync(
            SnippetTriggerPrefixInputDto inputDto,
            CancellationToken cancellationToken = default);

        Task<PagedResponse<SnippetTriggerPrefixDto>> GetPagedUserSnippetPrefixesAsync(
            GetUserSnippetTriggerPrefixesQuery query,
            Guid userId,
            CancellationToken cancellationToken);

        Task<PagedResponse<SnippetTriggerPrefixDto>> GetPagedWorkspaceSnippetTriggerPrefixesAsync(
            GetWorkspaceSnippetTriggerPrefixesQuery query,
            Guid workspaceId,
            CancellationToken cancellationToken);

        Task<IEnumerable<SnippetTriggerPrefixDto>> GetUserSnippetTriggerPrefixesAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<SnippetTriggerPrefixDto>> GetWorkspaceSnippetTriggerPrefixesAsync(
            Guid workspaceId,
            Guid executingUserId,
            CancellationToken cancellationToken = default);
        Task<SnippetTriggerPrefixDto> UpdateSnippetTriggerPrefixAsync(SnippetTriggerPrefixInputDto inputDto, CancellationToken cancellationToken = default);
    }
}