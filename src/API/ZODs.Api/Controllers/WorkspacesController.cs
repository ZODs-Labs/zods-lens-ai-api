using ZODs.Api.Common.Constants;
using ZODs.Api.Extensions;
using ZODs.Api.Filters;
using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Repository.QueryParams;
using ZODs.Api.Service;
using ZODs.Api.Service.Dtos;
using ZODs.Api.Service.InputDtos;
using ZODs.Api.Service.InputDtos.Snippet;
using ZODs.Api.Service.InputDtos.Workspace;
using ZODs.Api.Service.Validation.Interfaces;
using ZODs.Api.Validation;
using ZODs.Common.Attributes;
using ZODs.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ZODs.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class WorkspacesController : BaseController
    {
        private readonly IWorkspaceService workspaceService;
        private readonly IWorkspaceValidationService workspaceValidationService;
        private readonly ISnippetTriggerPrefixService snippetTriggerPrefixService;
        private readonly ISnippetsService snippetsService;

        public WorkspacesController(
            IWorkspaceService workspaceService,
            ISnippetTriggerPrefixService snippetTriggerPrefixService,
            ISnippetsService snippetsService,
            IWorkspaceValidationService workspaceValidationService)
        {
            this.workspaceService = workspaceService;
            this.snippetTriggerPrefixService = snippetTriggerPrefixService;
            this.snippetsService = snippetsService;
            this.workspaceValidationService = workspaceValidationService;
        }

        [HttpGet("{workspaceId}")]
        public async Task<IActionResult> GetUserWorkspace([NotEmptyGuid] Guid workspaceId, CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();

            var workspace = await this.workspaceService.GetWorkspaceDetailsAsync(workspaceId, userId, cancellationToken).NoSync();
            return OkApiResponse(workspace);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetUserWorkspaces(
            [FromQuery] GetUserWorkspacesQuery query,
            CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            var pagedWorkspaces = await this.workspaceService.GetUserWorkspacesAsync(userId, query, cancellationToken).NoSync();

            return OkApiResponse(pagedWorkspaces);
        }

        [HttpGet("me/widget")]
        public async Task<IActionResult> GetUserWorkspacesForWidget(
            [FromQuery] GetUserWorkspacesQuery query,
            CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            var pagedWorkspaces = await this.workspaceService.GetUserWorkspacesForWidgetAsync(query, userId, cancellationToken).NoSync();

            return OkApiResponse(pagedWorkspaces);
        }

        [HttpGet("me/dropdown")]
        public async Task<IActionResult> GetUserWorkspacesDropdown(
            [FromQuery][ValidEnum] WorkspaceMemberRoleIndex roleIndex,
            [FromQuery] bool forSnippetCreate,
            CancellationToken cancellationToken)
        {
            var executingUserId = User.GetUserId();

            var workspaces = await this.workspaceService.GetUserWorkspacesDropdownAsync(
                  executingUserId,
                  roleIndex,
                  forSnippetCreate,
                  cancellationToken).NoSync();

            return OkApiResponse(workspaces);
        }

        [HttpGet("{workspaceId}/members")]
        public async Task<IActionResult> GetWorkspaceMembers(
             [NotEmptyGuid] Guid workspaceId,
             [FromQuery] GetPagedWorkspaceMembersQuery query,
             CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            var pagedWorkspaceMembers = await this.workspaceService.GetWorkspaceMembersAsync(
                               workspaceId,
                               userId,
                               query,
                               cancellationToken).NoSync();

            return OkApiResponse(pagedWorkspaceMembers);
        }

        [HttpGet("{workspaceId}/members/invited")]
        public async Task<IActionResult> GetWorkspaceInvitedMembers(
            [NotEmptyGuid] Guid workspaceId,
            [FromQuery] GetPagedWorkspaceInvitedMembersQuery query,
            CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();

            // Only workspace owner can see invited members
            await workspaceValidationService.ValidateUserIsWorkspaceOwner(workspaceId, userId, cancellationToken).NoSync();

            var pagedWorkspaceInvitedMembers = await this.workspaceService.GetPagedWorkspaceInvitedMembersAsync(
                                 workspaceId,
                                 userId,
                                 query,
                                 cancellationToken).NoSync();

            return OkApiResponse(pagedWorkspaceInvitedMembers);
        }

        [HttpPost]
        [ValidateFeature(
            FeatureKeys.Workspaces,
            limitationIndexes: new[] { FeatureLimitationIndex.MaxWorkspaces })]
        [SyncFeatureLimitation(new[]
        {
            FeatureLimitationIndex.MaxWorkspaces,
            FeatureLimitationIndex.MaxWorkspaceInvites,
            FeatureLimitationIndex.MaxWorkspaceSnippets,
            FeatureLimitationIndex.MaxWorkspaceSnippetPrefixes,
        })]
        public async Task<IActionResult> CreateWorkspace(
             [FromBody] WorkspaceDto workspaceDto,
             CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();

            var workspace = await this.workspaceService.CreateWorkspaceAsync(workspaceDto, userId, cancellationToken).NoSync();
            return OkApiResponse(workspace);
        }

        [HttpPut("{workspaceId}")]
        public async Task<IActionResult> UpdateWorkspace(
             [NotEmptyGuid] Guid workspaceId,
             [FromBody] WorkspaceDto workspaceDto,
             CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();

            await workspaceValidationService.ValidateUserIsWorkspaceOwner(workspaceDto.Id, userId, cancellationToken);

            workspaceDto.Id = workspaceId;

            var workspace = await this.workspaceService.UpdateWorkspaceAsync(workspaceDto, userId, cancellationToken).NoSync();
            return OkApiResponse(workspace);
        }

        [HttpPost("{workspaceId}/members/invite")]
        [ValidateFeature(
            FeatureKeys.Workspaces,
            limitationIndexes: new[] { FeatureLimitationIndex.MaxWorkspaceInvites })]
        [SyncFeatureLimitation(FeatureLimitationIndex.MaxWorkspaceInvites)]
        public async Task<IActionResult> InviteMemberToWorkspace(
              [NotEmptyGuid] Guid workspaceId,
              [FromBody] InviteWorkspaceMemberInputDto inputDto,
              CancellationToken cancellationToken)
        {
            var executingUserId = User.GetUserId();

            await workspaceValidationService.ValidateUserIsWorkspaceOwner(workspaceId, executingUserId, cancellationToken).NoSync();

            await this.workspaceService.InviteMemberToWorkspaceAsync(workspaceId, inputDto, executingUserId, cancellationToken).NoSync();
            return this.Ok();
        }

        [HttpGet("{workspaceId}/snippets")]
        public async Task<IActionResult> GetWorkspaceSnippets(
            [NotEmptyGuid] Guid workspaceId,
            [FromQuery] GetWorkspaceSnippetsQuery query,
            CancellationToken cancellationToken)
        {
            var executingUserId = User.GetUserId();

            await workspaceValidationService.ValidateUserIsWorkspaceMember(workspaceId, executingUserId, cancellationToken).NoSync();

            if (query == null)
            {
                return BadRequest("You must provide query.");
            }

            var snippets = await this.workspaceService.GetWorkspaceSnippetsAsync(workspaceId, executingUserId, query, cancellationToken).NoSync();
            return OkApiResponse(snippets);
        }

        [HttpPost("{workspaceId}/snippets")]
        [ValidateFeature(
            FeatureKeys.Workspaces, 
            limitationIndexes: new[] { FeatureLimitationIndex.MaxWorkspaceSnippets })]
        [SyncFeatureLimitation(FeatureLimitationIndex.MaxWorkspaceSnippets)]
        public async Task<IActionResult> CreateSnippet(
            [NotEmptyGuid] Guid workspaceId,
            [FromBody] UpsertSnippetInputDto inputDto,
            CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            inputDto.Id = null;

            inputDto.IsWorkspaceOwned = true;
            inputDto.WorkspaceId = workspaceId;

            var snippet = await this.snippetsService.CreateSnippetAsync(inputDto, userId, cancellationToken);

            return OkApiResponse(snippet);
        }

        [HttpDelete("{workspaceId}/members/{workspaceMemberId}")]
        public async Task<IActionResult> RemoveMemberFromWorkspace(
            [NotEmptyGuid] Guid workspaceId,
            [NotEmptyGuid] Guid workspaceMemberId,
            CancellationToken cancellationToken)
        {
            var executingUserId = User.GetUserId();

            await workspaceValidationService.ValidateUserIsWorkspaceOwner(workspaceId, executingUserId, cancellationToken).NoSync();

            await this.workspaceService.RemoveMemberFromWorkspaceAsync(
                workspaceMemberId,
                cancellationToken).NoSync();

            return this.Ok();
        }

        // TODO: Fix case when user does not have valid subscription
        [HttpPost("members/invite/{inviteId}")]
        public async Task<IActionResult> AcceptWorkspaceMemberInvite(
            [NotEmptyGuid] Guid inviteId,
            CancellationToken cancellationToken)
        {
            var executingUserId = User.GetUserId();

            await workspaceValidationService.ValidateUserHasValidWorkspaceInvite(executingUserId, inviteId, cancellationToken).NoSync();

            await workspaceService.AcceptWorkspaceMemberInviteAsync(inviteId, cancellationToken).NoSync();

            return this.Ok();
        }

        [AllowAnonymous]
        [HttpGet("invites/{workspaceInviteId}/valid")]
        public async Task<IActionResult> IsWorkspaceMemberInviteValid(
            [NotEmptyGuid] Guid workspaceInviteId,
            CancellationToken cancellationToken)
        {
            var isValid = await this.workspaceService.IsWorkspaceMemberInviteValidAsync(workspaceInviteId, cancellationToken).NoSync();

            return OkApiResponse(new
            {
                IsValid = isValid,
            });
        }

        [AllowAnonymous]
        [HttpGet("invites/{workspaceInviteId}/info")]
        public async Task<IActionResult> GetWorkspaceInviteInfo(
            [NotEmptyGuid] Guid workspaceInviteId,
            CancellationToken cancellationToken)
        {
            var workspaceInviteInfo = await this.workspaceService.GetWorkspaceInviteInfoAsync(workspaceInviteId, cancellationToken).NoSync();

            return OkApiResponse(workspaceInviteInfo);
        }

        [HttpPost("{workspaceId}/snippets/prefixes")]
        [ValidateFeature(
            FeatureKeys.WorkspaceSnippetPrefixes,
            limitationIndexes: new[] { FeatureLimitationIndex.MaxWorkspaceSnippetPrefixes })]
        [SyncFeatureLimitation(FeatureLimitationIndex.MaxWorkspaceSnippetPrefixes)]
        public async Task<IActionResult> CreateWorkspaceSnippetTriggerPrefix(
            [NotEmptyGuid] Guid workspaceId,
            [Required][FromBody] SnippetTriggerPrefixInputDto inputDto,
            CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();

            inputDto.WorkspaceId = workspaceId;

            await workspaceValidationService.ValidateUserIsWorkspaceOwner(workspaceId, userId, cancellationToken);

            var dto = await this.snippetTriggerPrefixService.CreateSnippetTriggerPrefixAsync(
                  inputDto,
                  cancellationToken).NoSync();

            return OkApiResponse(dto);
        }

        [HttpGet("{workspaceId}/snippets/prefixes")]
        public async Task<IActionResult> GetWorkspaceSnippetTriggerPrefixes(
            [NotEmptyGuid] Guid workspaceId,
            [FromQuery] GetWorkspaceSnippetTriggerPrefixesQuery query,
            CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            await workspaceValidationService.ValidateUserIsWorkspaceMember(workspaceId, userId, cancellationToken);

            var pagedPrefixes = await this.snippetTriggerPrefixService.GetPagedWorkspaceSnippetTriggerPrefixesAsync(
                query,
                workspaceId,
                cancellationToken);

            return OkApiResponse(pagedPrefixes);
        }

        [HttpGet("{workspaceId}/snippets/prefixes/dropdown")]
        public async Task<IActionResult> GetWorkspaceSnippetTriggerPrefixesDropdown(
            [NotEmptyGuid] Guid workspaceId,
            CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            var allPrefixes = await this.snippetTriggerPrefixService.GetWorkspaceSnippetTriggerPrefixesAsync(
                workspaceId,
                userId,
                cancellationToken);

            return OkApiResponse(allPrefixes);
        }
    }
}
