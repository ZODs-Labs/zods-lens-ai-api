using System.Web;
using ZODs.Common.Exceptions;
using ZODs.Api.Common.Configuration;
using ZODs.Api.Common.Extensions;
using ZODs.Api.Common.Interfaces;
using ZODs.Api.Repository;
using ZODs.Api.Repository.Dtos;
using ZODs.Api.Repository.Entities;
using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Repository.Interfaces;
using ZODs.Api.Repository.QueryParams;
using ZODs.Api.Service.Common;
using ZODs.Api.Service.Dtos;
using ZODs.Api.Service.InputDtos.Workspace;
using ZODs.Api.Service.Mappers;
using ZODs.Api.Service.Validation.Interfaces;
using ZODs.Common.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ZODs.Api.Service;

public sealed class WorkspaceService : IWorkspaceService
{
    private readonly IUnitOfWork<ZodsContext> unitOfWork;
    private readonly IServiceProvider serviceProvider;
    private readonly IEmailService emailService;
    private readonly IWorkspaceValidationService workspaceValidationService;
    private readonly IFeatureLimitationService featureLimitationService;
    private readonly ILogger<WorkspaceService> logger;
    private readonly ZodsApiConfiguration zodsApiConfiguration;

    public WorkspaceService(
        IUnitOfWork<ZodsContext> unitOfWork,
        ILogger<WorkspaceService> logger,
        IServiceProvider serviceProvider,
        IEmailService emailService,
        IOptions<ZodsApiConfiguration> options,
        IWorkspaceValidationService workspaceValidationService,
        IFeatureLimitationService featureLimitationService)
    {
        this.unitOfWork = unitOfWork;
        this.logger = logger;
        this.serviceProvider = serviceProvider;
        this.emailService = emailService;

        this.zodsApiConfiguration = options.Value;
        if (this.zodsApiConfiguration == null)
        {
            throw new ArgumentNullException(nameof(options), $"Missing configuration for {nameof(ZodsApiConfiguration)}");
        }

        this.workspaceValidationService = workspaceValidationService;
        this.featureLimitationService = featureLimitationService;
    }

    private IWorkspacesRepository WorkspacesRepository => this.unitOfWork.GetRepository<IWorkspacesRepository>();
    private IWorkspaceMemberInvitesRepository WorkspaceMemberInvitesRepository => this.unitOfWork.GetRepository<IWorkspaceMemberInvitesRepository>();
    private ISnippetsRepository SnippetsRepository => this.unitOfWork.GetRepository<ISnippetsRepository>();
    private IUsersRepository UsersRepository => this.unitOfWork.GetRepository<IUsersRepository>();

    public async Task<WorkspaceDto> GetWorkspaceById(Guid id, CancellationToken cancellationToken)
    {
        var workspace = await this.WorkspacesRepository.GetByIdAsync(id, cancellationToken: cancellationToken).NoSync();
        if (workspace == null)
        {
            throw new KeyNotFoundException(typeof(Workspace).NotFoundValidationMessage(id));
        }

        return workspace.ToDto();
    }

    public async Task<UserWorkspaceDto> GetWorkspaceDetailsAsync(
        Guid id,
        Guid executingUserId,
        CancellationToken cancellationToken)
    {
        var workspaceDetails = await this.WorkspacesRepository.FirstOrDefaultAsync(
            x => x.Id == id &&
                 x.Members.Any(m => m.UserId == executingUserId),
            x => new UserWorkspaceDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                LastUpdatedAt = x.ModifiedAt ?? x.CreatedAt,
                RoleIndex = x.Members.Where(m => m.UserId == executingUserId)
                                         .Select(m => m.Roles.Select(r => r.Role.Index))
                                         .SelectMany(r => r)
                                         .First(),
            },
            cancellationToken: cancellationToken);

        if (workspaceDetails == null)
        {
            throw new KeyNotFoundException(typeof(Workspace).NotFoundValidationMessage(id));
        }

        workspaceDetails.IsOwner = workspaceDetails.RoleIndex == WorkspaceMemberRoleIndex.Owner;

        return workspaceDetails;
    }

    public async Task<UserWorkspacesDto> GetUserWorkspacesAsync(
        Guid userId,
        GetUserWorkspacesQuery query,
        CancellationToken cancellationToken)
    {
        if (query == null)
        {
            throw new BusinessValidationException("Query cannot be null.");
        }

        this.logger.LogInformation("Getting workspaces for user {userId}", userId);

        var entities = await this.WorkspacesRepository.GetUserWorkspaces(userId, query, cancellationToken).NoSync();
        var userWorkspacesDto = new UserWorkspacesDto
        {
            OwnedWorkspaces = entities.Where(x => x.RoleIndex == WorkspaceMemberRoleIndex.Owner).ToList(),
            MemberWorkspaces = entities.Where(x => x.RoleIndex != WorkspaceMemberRoleIndex.Owner).ToList(),
        };

        return userWorkspacesDto;
    }

    public async Task<UserWorkspacesDto> GetUserWorkspacesForWidgetAsync(
        GetUserWorkspacesQuery query,
        Guid userId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var entities = await this.WorkspacesRepository.GetUserWorkspacesForWidgetAsync(
            query, 
            userId,
            cancellationToken).NoSync();

        var userWorkspacesDto = new UserWorkspacesDto
        {
            OwnedWorkspaces = entities.Where(x => x.RoleIndex == WorkspaceMemberRoleIndex.Owner).ToList(),
            MemberWorkspaces = entities.Where(x => x.RoleIndex != WorkspaceMemberRoleIndex.Owner).ToList(),
        };

        return userWorkspacesDto;
    }

    public async Task<PagedResponse<WorkspaceMemberDto>> GetWorkspaceMembersAsync(
        Guid workspaceId,
        Guid userId,
        GetPagedWorkspaceMembersQuery query,
        CancellationToken cancellationToken)
    {
        if (query == null)
        {
            throw new BusinessValidationException("Query cannot be null.");
        }

        logger.LogInformation("Getting workspace members for workspace {workspaceId} initiated by user {userId}", workspaceId, userId);
        var pagedWorkspaceMembers = await this.WorkspacesRepository.GetPagedWorkspaceMembersAsync(
            workspaceId,
            userId,
            query,
            cancellationToken).NoSync();

        var pagedResponse = pagedWorkspaceMembers.ToDto();
        return pagedResponse;
    }

    public async Task<ICollection<WorkspaceDropdownDto>> GetUserWorkspacesDropdownAsync(
        Guid userId,
        WorkspaceMemberRoleIndex roleIndex,
        bool forSnippetCreate,
        CancellationToken cancellationToken)
    {
        var userWorkspaces = await this.WorkspacesRepository.GetUserWorkspacesDropdownAsync(
               userId,
               roleIndex,
               cancellationToken).NoSync();

        if (forSnippetCreate)
        {
            var workspaceSnippetsToCreateLeft = await this.featureLimitationService.GetUserFeatureLimitationUsageAsync<Dictionary<Guid, int>>(
                    FeatureLimitationIndex.MaxWorkspaceSnippets,
                    FeatureLimitationContext.Create(userId),
                    cancellationToken).NoSync();

            foreach (var workspace in userWorkspaces)
            {
                if (workspaceSnippetsToCreateLeft.TryGetValue(workspace.Id, out var snippetsToCreateLeft))
                {
                    workspace.IsReachedMaxSnippetsLimitation = snippetsToCreateLeft <= 0;
                }
                else
                {
                    workspace.IsReachedMaxSnippetsLimitation = true;
                }
            }

            userWorkspaces = userWorkspaces.OrderBy(x => x.IsReachedMaxSnippetsLimitation).ToList();
        }

        return userWorkspaces;
    }

    public async Task<PagedResponse<WorkspaceInviteMemberDto>> GetPagedWorkspaceInvitedMembersAsync(
        Guid workspaceId,
        Guid executingUserId,
        GetPagedWorkspaceInvitedMembersQuery query,
        CancellationToken cancellationToken)
    {
        if (query == null)
        {
            throw new BusinessValidationException("Query cannot be null.");
        }

        logger.LogInformation("Getting workspace invited members for workspace {workspaceId} initiated by user {userId}", workspaceId, executingUserId);

        var pagedWorkspaceInvitedMembers = await this.WorkspaceMemberInvitesRepository.GetWorkspaceInvitedMembersAsync(
                  workspaceId,
                  executingUserId,
                  query,
                  cancellationToken).NoSync();

        var pagedResponse = pagedWorkspaceInvitedMembers.ToDto();

        return pagedResponse;
    }

    public async Task<WorkspaceDto> CreateWorkspaceAsync(
        WorkspaceDto workspaceDto,
        Guid executingUserId,
        CancellationToken cancellationToken)
    {
        await this.workspaceValidationService.ValidateWorkspaceDtoForUpsert(workspaceDto, executingUserId, cancellationToken).NoSync();

        var entity = workspaceDto.ToEntity();
        entity.CreatedBy = executingUserId.ToString();

        // Workspace is active by default
        entity.IsActive = true;

        logger.LogInformation("Creating workspace {workspaceName}", entity.Name);

        entity = await this.WorkspacesRepository.Insert(entity, cancellationToken).NoSync();
        await this.unitOfWork.CommitAsync(cancellationToken).NoSync();

        var workspaceMember = await CreateWorkspaceMemberEntity(
            executingUserId,
            workspaceId: entity.Id,
            executingUserId,
            WorkspaceMemberRoleIndex.Owner,
            cancellationToken).NoSync();
        await this.WorkspacesRepository.AddMemberWithRoleToWorkspaceAsync(workspaceMember, cancellationToken)
                                       .NoSync();

        await this.unitOfWork.CommitAsync(cancellationToken).NoSync();

        workspaceDto = entity.ToDto();

        return workspaceDto;
    }

    public async Task<WorkspaceDto> UpdateWorkspaceAsync(
         WorkspaceDto workspaceDto,
         Guid userId,
         CancellationToken cancellationToken)
    {
        await this.workspaceValidationService.ValidateWorkspaceExists(workspaceDto.Id, cancellationToken);
        await this.workspaceValidationService.ValidateWorkspaceDtoForUpsert(workspaceDto, userId, cancellationToken);

        // Workspace is active by default
        workspaceDto.IsActive = true;

        var entity = workspaceDto.ToEntity();
        entity.ModifiedBy = userId.ToString();

        logger.LogInformation("Updating workspace with id {workspaceId}", workspaceDto.Id);

        await this.WorkspacesRepository.Update(entity, cancellationToken).NoSync();
        await this.unitOfWork.CommitAsync(cancellationToken).NoSync();

        return workspaceDto;
    }

    public async Task<PagedResponse<SnippetOverviewDto>> GetWorkspaceSnippetsAsync(
        Guid workspaceId,
        Guid executingUserId,
        GetWorkspaceSnippetsQuery query,
        CancellationToken cancellationToken)
    {
        typeof(Snippet).HasProperty(query.SortBy);

        var paginatedSnippets = await SnippetsRepository.GetWorkspaceSnippetsOverviewAsync(
            workspaceId,
            executingUserId,
            query,
            cancellationToken).NoSync();

        var pagedResponse = paginatedSnippets.ToPagedResponse();
        return pagedResponse;
    }

    public async Task RemoveMemberFromWorkspaceAsync(
        Guid workspaceMemberId,
        CancellationToken cancellationToken)
    {
        await WorkspacesRepository.RemoveWorkspaceMemberAsync(workspaceMemberId, cancellationToken).NoSync();
    }

    public async Task InviteMemberToWorkspaceAsync(
        Guid workspaceId,
        InviteWorkspaceMemberInputDto inputDto,
        Guid executingUserId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(inputDto);

        var email = inputDto.Email;

        await workspaceValidationService.ValidateMemberInvitationDoesNotExists(workspaceId, email, cancellationToken).NoSync();
        await workspaceValidationService.ValidateMemberToAddRoleIsLowerThanExecutingUser(
                 workspaceId,
                 executingUserId,
                 inputDto.RoleIndex,
                 cancellationToken).NoSync();
        await this.workspaceValidationService.ValidateWorkspaceExists(workspaceId, cancellationToken).NoSync();

        logger.LogInformation("Inviting member {email} to workspace {workspaceId} with role {roleIndex}", email, workspaceId, inputDto.RoleIndex);

        var workspaceMemberInvite = new WorkspaceMemberInvite
        {
            Email = email,
            WorkspaceId = workspaceId,
            RoleIndex = inputDto.RoleIndex,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            InvitedByUserId = executingUserId,
            CreatedBy = executingUserId.ToString(),
        };
        workspaceMemberInvite = await this.WorkspaceMemberInvitesRepository.Insert(workspaceMemberInvite, cancellationToken).NoSync();
        await this.unitOfWork.CommitAsync(cancellationToken).NoSync();

        var inviteUrl = ComposeWorkspaceMemberInviteUrl(workspaceId, workspaceMemberInvite.Id);
        var workspaceName = await WorkspacesRepository.FirstOrDefaultAsync(
              w => w.Id == workspaceId,
              w => w.Name,
              cancellationToken: cancellationToken).NoSync();

        if (string.IsNullOrWhiteSpace(workspaceName))
        {
            throw new BusinessValidationException(typeof(Workspace).NotFoundValidationMessage(workspaceId));
        }

        await emailService.SendWorkspaceMemberInvitationEmailAsync(
            workspaceMemberInvite.Email,
            inviteUrl,
            workspaceName,
            cancellationToken).NoSync();
    }

    public async Task<bool> IsWorkspaceMemberInviteValidAsync(
        Guid workspaceInviteId,
        CancellationToken cancellationToken)
    {
        var isInviteValid = await WorkspaceMemberInvitesRepository.ExistsAsync(
            x => x.Id == workspaceInviteId && !x.IsUsed && x.ExpiresAt > DateTime.UtcNow,
            cancellationToken: cancellationToken).NoSync();

        return isInviteValid;
    }

    public async Task AcceptWorkspaceMemberInviteAsync(
        Guid workspaceInviteId,
        CancellationToken cancellationToken)
    {
        var workspaceInvite = await WorkspaceMemberInvitesRepository.FirstOrDefaultAsync(
                       x => x.Id == workspaceInviteId,
                       cancellationToken: cancellationToken).NoSync();
        if (workspaceInvite == null)
        {
            throw new KeyNotFoundException(typeof(WorkspaceMemberInvite).NotFoundValidationMessage(workspaceInviteId));
        }

        var memberToAddUserId = await UsersRepository.FirstOrDefaultAsync(
                                   x => x.Email == workspaceInvite.Email,
                                   x => x.Id,
                                   cancellationToken: cancellationToken).NoSync();
        if (memberToAddUserId == default)
        {
            throw new BusinessValidationException($"User with email {workspaceInvite.Email} not found.");
        }

        workspaceInvite.IsUsed = true;
        workspaceInvite.AcceptedAt = DateTime.UtcNow;
        workspaceInvite.AcceptedByUserId = memberToAddUserId;

        await this.WorkspaceMemberInvitesRepository.Update(workspaceInvite, cancellationToken).NoSync();
        await this.unitOfWork.CommitAsync(cancellationToken).NoSync();

        var addMemberToWorkspaceDto = new AddMemberToWorkspaceInputDto
        {
            WorkspaceId = workspaceInvite.WorkspaceId,
            MemberUserId = memberToAddUserId,
            RoleIndex = workspaceInvite.RoleIndex,
        };

        await AddMemberToWorkspace(
            addMemberToWorkspaceDto,
            executingUserId: null,
            cancellationToken);
    }

    public async Task<WorkspaceInviteInfoDto> GetWorkspaceInviteInfoAsync(
        Guid workspaceInviteId,
        CancellationToken cancellationToken)
    {
        var invite = await WorkspaceMemberInvitesRepository.FirstOrDefaultAsync(
                x => x.Id == workspaceInviteId,
                x => new
                {
                    x.Email,
                    x.WorkspaceId,
                },
            cancellationToken: cancellationToken).NoSync();

        if (invite == null)
        {
            throw new KeyNotFoundException(typeof(WorkspaceMemberInvite).NotFoundValidationMessage(workspaceInviteId));
        }

        var workspaceName = await WorkspacesRepository.FirstOrDefaultAsync(
                w => w.Id == invite.WorkspaceId,
                w => w.Name,
                cancellationToken: cancellationToken).NoSync();

        if (string.IsNullOrWhiteSpace(workspaceName))
        {
            throw new BusinessValidationException("Workspace for invite with id {workspaceInviteId} not found.");
        }

        return new WorkspaceInviteInfoDto
        {
            InviteEmail = invite.Email,
            WorkspaceName = workspaceName,
        };
    }

    private async Task<WorkspaceMember> CreateWorkspaceMemberEntity(
        Guid memberUserId,
        Guid workspaceId,
        Guid? executingUserId,
        WorkspaceMemberRoleIndex roleIndex,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting role id for role index {roleIndex}", roleIndex);
        var roleId = await this.WorkspacesRepository.GetRoleIdByIndexAsync(roleIndex, cancellationToken).NoSync();

        var workspaceMember = new WorkspaceMember
        {
            UserId = memberUserId,
            WorkspaceId = workspaceId,
            CreatedBy = executingUserId?.ToString() ?? "system",
            Roles = new List<WorkspaceMemberRole>
           {
                new WorkspaceMemberRole
                {
                    WorkspaceRoleId = roleId,
                },
           },
        };

        return workspaceMember;
    }

    private async Task AddMemberToWorkspace(
        AddMemberToWorkspaceInputDto dto,
        Guid? executingUserId,
        CancellationToken cancellationToken)
    {
        var workspaceMember = await CreateWorkspaceMemberEntity(
                dto.MemberUserId,
                dto.WorkspaceId,
                executingUserId,
                dto.RoleIndex,
                cancellationToken).NoSync();

        logger.LogInformation("Adding member {memberUserId} to workspace {workspaceId} with role {roleIndex}", dto.MemberUserId, dto.WorkspaceId, dto.RoleIndex);
        await this.WorkspacesRepository.AddMemberWithRoleToWorkspaceAsync(workspaceMember, cancellationToken)
                                .NoSync();
        await this.unitOfWork.CommitAsync(cancellationToken).NoSync();
    }

    private string ComposeWorkspaceMemberInviteUrl(Guid workspaceId, Guid inviteId)
    {
        var uriBuilder = new UriBuilder(zodsApiConfiguration.WebUrl);
        uriBuilder.Path += "/workspaces/invite";
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);

        query["workspaceId"] = HttpUtility.UrlEncode(workspaceId.ToString());
        query["inviteId"] = HttpUtility.UrlEncode(inviteId.ToString());

        uriBuilder.Query = query.ToString();

        return uriBuilder.ToString();
    }
}
