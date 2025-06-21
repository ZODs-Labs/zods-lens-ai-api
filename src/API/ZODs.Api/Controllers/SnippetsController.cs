using ZODs.Api.Common.Constants;
using ZODs.Api.Extensions;
using ZODs.Api.Filters;
using ZODs.Api.Models;
using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Repository.QueryParams;
using ZODs.Api.Service;
using ZODs.Api.Service.InputDtos;
using ZODs.Api.Service.InputDtos.Snippet;
using ZODs.Api.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;
using ZODs.Common.Models;
using ZODs.Api.Common.Attributes;

namespace ZODs.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SnippetsController : BaseController
    {
        private readonly ISnippetsService snippetsService;
        private readonly ISnippetTriggerPrefixService snippetTriggerPrefixService;

        public SnippetsController(
            ISnippetsService snippetsService,
            ISnippetTriggerPrefixService snippetTriggerPrefixService)
        {
            this.snippetsService = snippetsService;
            this.snippetTriggerPrefixService = snippetTriggerPrefixService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetUserSnippets(
            [FromQuery] GetUserSnippetsQuery query,
            CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            var snippets = await this.snippetsService.GetUserSnippetsOverviewAsync(query, userId, cancellationToken);

            return OkApiResponse(snippets);
        }

        [HttpGet("me/shared")]
        public async Task<IActionResult> GetSnippetsSharedWithUser(
            [FromQuery] GetUserSnippetsQuery query,
            CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            var snippets = await this.snippetsService.GetSnippetsSharedWithUserAsync(query, userId, cancellationToken);

            return OkApiResponse(snippets);
        }

        [AllowNoSubscription]
        [HttpGet("me/own")]
        public async Task<IActionResult> GetSnippetsOwnedByUser(
             [FromQuery] GetUserSnippetsQuery query,
             CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            var snippets = await this.snippetsService.GetUserOwnSnippetsAsync(query, userId, cancellationToken);

            return OkApiResponse(snippets);
        }

        [HttpGet("me/full")]
        public async Task<IActionResult> GetFullUserSnippets(CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();

            var snippets = await this.snippetsService.GetUserSnippetsForIntegrationAsync(userId, cancellationToken).ConfigureAwait(false);
            return OkApiResponse(snippets);
        }

        [HttpGet("{id}/code")]
        public async Task<IActionResult> GetSnippetCode(Guid id, CancellationToken cancellationToken)
        {
            if (id == default)
            {
                return BadRequest(ApiResponse.Error("Id is required.", HttpStatusCode.BadRequest));
            }

            var userId = User.GetUserId();
            var code = await this.snippetsService.GetSnippetCodeAsync(id, userId, cancellationToken);

            return OkApiResponse(code);
        }

        [HttpPost]
        [ValidateFeature(
            FeatureKeys.PersonalSnippets,
            limitationIndexes: new[] { FeatureLimitationIndex.MaxPersonalSnippets })]
        [SyncFeatureLimitation(FeatureLimitationIndex.MaxPersonalSnippets)]
        public async Task<IActionResult> CreateSnippet(
            [FromBody] UpsertSnippetInputDto inputDto,
            CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            inputDto.Id = null;

            inputDto.IsWorkspaceOwned = false;
            inputDto.WorkspaceId = null;

            var snippet = await this.snippetsService.CreateSnippetAsync(inputDto, userId, cancellationToken);

            return OkApiResponse(snippet);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSnippet(Guid? id, [FromBody] UpsertSnippetInputDto inputDto, CancellationToken cancellationToken)
        {
            if (id == null)
            {
                return UnprocessableEntity(ApiResponse.Error("Id is required.", HttpStatusCode.UnprocessableEntity));
            }

            var userId = User.GetUserId();
            inputDto.Id = id;
            var snippetDto = await this.snippetsService.UpdateSnippetAsync(inputDto, userId, cancellationToken);
            return OkApiResponse(snippetDto);
        }

        [HttpGet("me/version")]
        public async Task<long> GetUserSnippetsVersion(CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            var version = await this.snippetsService.GetUserSnippetsVersionAsync(userId, cancellationToken);

            return version;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserSnippetById(Guid id, CancellationToken cancellationToken)
        {
            if (id == default)
            {
                return BadRequest(ApiResponse.Error("Id is required.", HttpStatusCode.BadRequest));
            }

            var userId = User.GetUserId();
            var snippet = await this.snippetsService.GetUserSnippetByIdAsync(id, userId, cancellationToken);

            return OkApiResponse(snippet);
        }

        [HttpPost("me/prefixes")]
        [ValidateFeature(
            FeatureKeys.PersonalSnippetPrefixes,
            limitationIndexes: new[] { FeatureLimitationIndex.MaxPersonalSnippetPrefixes })]
        [SyncFeatureLimitation(FeatureLimitationIndex.MaxPersonalSnippetPrefixes)]
        public async Task<IActionResult> CreateUserSnippetTriggerPrefix(
             [Required][FromBody] SnippetTriggerPrefixInputDto inputDto,
             CancellationToken cancellationToken)
        {
            inputDto.UserId = User.GetUserId();

            var prefixDto = await this.snippetTriggerPrefixService.CreateSnippetTriggerPrefixAsync(inputDto, cancellationToken);

            return OkApiResponse(prefixDto);
        }

        [HttpPut("prefixes/{id}")]
        public async Task<IActionResult> UpdateUserSnippetTriggerPrefix(
            [Required][FromRoute] Guid id,
            [Required][FromBody] SnippetTriggerPrefixInputDto inputDto,
            CancellationToken cancellationToken)
        {
            inputDto.Id = id;
            inputDto.UserId = User.GetUserId();

            var prefixDto = await this.snippetTriggerPrefixService.UpdateSnippetTriggerPrefixAsync(inputDto, cancellationToken);

            return OkApiResponse(prefixDto);
        }

        [HttpGet("me/prefixes")]
        public async Task<IActionResult> GetUserSnippetTriggerPrefixes(
            [FromQuery] GetUserSnippetTriggerPrefixesQuery query,
            CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            var pagedPrefixes = await this.snippetTriggerPrefixService.GetPagedUserSnippetPrefixesAsync(query, userId, cancellationToken);

            return OkApiResponse(pagedPrefixes);
        }

        [HttpGet("me/prefixes/dropdown")]
        public async Task<IActionResult> GetUserSnippetTriggerPrefixesDropdown(
            CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            var allPrefixes = await this.snippetTriggerPrefixService.GetUserSnippetTriggerPrefixesAsync(userId, cancellationToken);

            return OkApiResponse(allPrefixes);
        }
    }
}
