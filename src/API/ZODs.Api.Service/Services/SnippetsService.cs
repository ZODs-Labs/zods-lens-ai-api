using ZODs.Api.Common.Extensions;
using ZODs.Api.Repository;
using ZODs.Api.Repository.Entities;
using ZODs.Api.Repository.Interfaces;
using ZODs.Api.Service.Dtos;
using ZODs.Api.Service.Mappers;
using ZODs.Api.Repository.QueryParams;
using ZODs.Api.Service.InputDtos.Snippet;
using ZODs.Common.Extensions;
using ZODs.Api.Service.Validation.Interfaces;

namespace ZODs.Api.Service;

public sealed class SnippetsService : ISnippetsService
{
    private readonly IUnitOfWork<ZodsContext> unitOfWork;
    private readonly ISnippetValidationService snippetValidationService;

    public SnippetsService(
        IUnitOfWork<ZodsContext> unitOfWork,
        ISnippetValidationService snippetValidationService)
    {
        this.unitOfWork = unitOfWork;
        this.snippetValidationService = snippetValidationService;
    }

    private ISnippetsRepository SnippetsRepository => this.unitOfWork.GetRepository<ISnippetsRepository>();
    private IWorkspacesRepository WorkspacesRepository => this.unitOfWork.GetRepository<IWorkspacesRepository>();

    public async Task<UserSnippetsOverviewDto> GetUserSnippetsOverviewAsync(GetUserSnippetsQuery query, Guid userId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));
        query.SortBy ??= nameof(Snippet.CreatedAt);

        ValidateSnippetSortProperty(query.SortBy);

        var snippets = await this.SnippetsRepository.GetUserSnippetsOverviewAsync(userId, query, cancellationToken);

        var pagedResponse = snippets.ToPagedResponse();

        var userSnippetsDto = new UserSnippetsOverviewDto(pagedResponse);

        return userSnippetsDto;
    }

    public async Task<UserSnippetsOverviewDto> GetSnippetsSharedWithUserAsync(GetUserSnippetsQuery query, Guid userId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));
        query.SortBy ??= nameof(Snippet.CreatedAt);

        ValidateSnippetSortProperty(query.SortBy);

        var snippets = await this.SnippetsRepository.GetSnippetsSharedWithUser(userId, query, cancellationToken);

        var pagedResponse = snippets.ToPagedResponse();

        var userSnippetsDto = new UserSnippetsOverviewDto(pagedResponse);

        return userSnippetsDto;
    }

    public async Task<UserSnippetsOverviewDto> GetUserOwnSnippetsAsync(GetUserSnippetsQuery query, Guid userId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));
        query.SortBy ??= nameof(Snippet.CreatedAt);

        ValidateSnippetSortProperty(query.SortBy);

        var snippets = await this.SnippetsRepository.GetUserOwnSnippets(userId, query, cancellationToken);

        var pagedResponse = snippets.ToPagedResponse();

        var userSnippetsDto = new UserSnippetsOverviewDto(pagedResponse);

        return userSnippetsDto;
    }

    public async Task<UserSnippetsForIntegrationDto> GetUserSnippetsForIntegrationAsync(Guid userId, CancellationToken cancellationToken)
    {
        var snippets = await SnippetsRepository.GetUserSnippetsForIntegrationAsync(userId, cancellationToken)
                                               .NoSync();

        var userSnippetsVersion = await this.SnippetsRepository.GetUserSnippetsVersionAsync(userId, cancellationToken);
        var workspaceSnippetsVersion = await this.WorkspacesRepository.GetAllUserWorkspacesSnippetVersionSumAsync(userId, cancellationToken);
        
        // Sum all snippets versions to get the total version
        // This is used to check if the user has the latest snippets version
        var snippetsVersionSum = userSnippetsVersion + workspaceSnippetsVersion;

        return new UserSnippetsForIntegrationDto
        {
            Snippets = snippets,
            SnippetsVersion = snippetsVersionSum,
        };
    }

    public async Task<string> GetSnippetCodeAsync(Guid snippetId, Guid userId, CancellationToken cancellationToken)
    {
        var code = await this.SnippetsRepository.GetSnippetCodeAsync(snippetId, userId, cancellationToken)
                                                .NoSync();
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new KeyNotFoundException($"Snippet with id {snippetId} not found.");
        }

        return code;
    }

    public async Task<SnippetDto> CreateSnippetAsync(UpsertSnippetInputDto inputDto, Guid userId, CancellationToken cancellationToken)
    {
        await this.snippetValidationService.ValidateSnippetDto(inputDto, userId, cancellationToken).NoSync();

        var snippet = inputDto.ToEntity();
        snippet.UserId = inputDto.IsWorkspaceOwned ? null : userId;
        snippet.CreatedBy = userId.ToString();

        await this.SnippetsRepository.CreateSnippetAsync(snippet, inputDto.WorkspaceId, cancellationToken).NoSync();
        await this.unitOfWork.CommitAsync(cancellationToken).NoSync();

        await IncrementSnippetsVersionAsync(userId, inputDto.WorkspaceId, inputDto.IsWorkspaceOwned, cancellationToken);

        return snippet.ToDto();
    }

    public async Task<SnippetDto> UpdateSnippetAsync(
        UpsertSnippetInputDto inputDto,
        Guid userId,
        CancellationToken cancellationToken)
    {
        await snippetValidationService.ValidateUserHasWritePermissionsForSnippet(inputDto.Id!.Value, userId, cancellationToken).NoSync();
        await this.snippetValidationService.ValidateSnippetDto(inputDto, userId, cancellationToken).NoSync();

        var snippet = inputDto.ToEntity();
        snippet.UserId = inputDto.IsWorkspaceOwned ? null : userId;
        snippet.ModifiedBy = userId.ToString();

        await this.SnippetsRepository.UpdateSnippetAsync(snippet, inputDto.WorkspaceId, cancellationToken).NoSync();
        await this.unitOfWork.CommitAsync(cancellationToken).NoSync();

        await IncrementSnippetsVersionAsync(userId, inputDto.WorkspaceId, inputDto.IsWorkspaceOwned, cancellationToken);

        return snippet.ToDto();
    }

    public async Task<long> GetUserSnippetsVersionAsync(Guid userId, CancellationToken cancellationToken)
    {
        var ownSnippetsVersion =  await this.SnippetsRepository.GetUserSnippetsVersionAsync(userId, cancellationToken);
        var workspaceSnippetsVersion = await this.WorkspacesRepository.GetAllUserWorkspacesSnippetVersionSumAsync(userId, cancellationToken);

        // Sum all snippets versions to get the total version
        var snippetsVersionSum = ownSnippetsVersion + workspaceSnippetsVersion;

        return snippetsVersionSum;
    }

    public async Task<SnippetDto> GetUserSnippetByIdAsync(
        Guid snippetId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var snippet = await this.SnippetsRepository.GetUserSnippetByIdAsync(
            snippetId,
            userId,
            cancellationToken).NoSync();

        if (snippet == null)
        {
            throw new KeyNotFoundException(typeof(Snippet).NotFoundValidationMessage(snippetId));
        }

        var snippetDto = snippet.ToDto();

        return snippetDto;
    }

    public async Task IncrementSnippetsVersionAsync(Guid userId, Guid? workspaceId, bool isWorkspaceOwned, CancellationToken cancellationToken)
    {
        if (isWorkspaceOwned && workspaceId != null)
        {
            await this.WorkspacesRepository.IncrementWorksaceSnippetsVersionAsync(workspaceId.Value, cancellationToken);
        }
        else
        {
            await this.SnippetsRepository.IncrementUserSnippetsVersion(userId, cancellationToken);
        }

        await this.unitOfWork.CommitAsync(cancellationToken);
    }

    private static void ValidateSnippetSortProperty(string sortBy)
           => typeof(Snippet).HasProperty(sortBy);
}