using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZODs.Api.Common.Attributes;
using ZODs.Api.Common.Constants;
using ZODs.Api.Extensions;
using ZODs.Api.Filters;
using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Repository.QueryParams;
using ZODs.Api.Service;
using ZODs.Api.Service.InputDtos;
using ZODs.Api.Service.InputDtos.AILens;
using ZODs.Api.Validation;
using ZODs.Common.Extensions;

namespace ZODs.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class AILensController(IAILensService aiLensService) : BaseController
{
    public readonly IAILensService aiLensService = aiLensService;

    [HttpGet("me/all")]
    public async Task<IActionResult> GetAllUserLensesInfoAsync(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var aiLenses = await aiLensService.GetUserAILensesInfoAsync(userId, cancellationToken);

        return OkApiResponse(aiLenses);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAILensAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var aiLens = await aiLensService.GetLensByIdAsync(id, userId, cancellationToken);

        return OkApiResponse(aiLens);
    }

    [AllowNoSubscription]
    [HttpGet("me")]
    public async Task<IActionResult> GetUserAILensesAsync(
        [FromQuery] GetPagedUserAILensesQuery query,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var aiLenses = await aiLensService.GatePagedUserAILensesAsync(query, userId, cancellationToken);
        return OkApiResponse(aiLenses);
    }

    [HttpGet("me/built-in")]
    public async Task<IActionResult> GetUserBuiltInLensesAsync(
        [FromQuery] GetUserBuiltInAILensesQuery query,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var aiLenses = await aiLensService.GetPagedUserBuiltInAILensesAsync(query, userId, cancellationToken);
        return OkApiResponse(aiLenses);
    }

    [ValidateFeature(
        FeatureKeys.AILens, 
        limitationIndexes: new[] { FeatureLimitationIndex.MaxAILenses })]
    [SyncFeatureLimitation(FeatureLimitationIndex.MaxAILenses)]
    [HttpPost]
    public async Task<IActionResult> CreateAILens(
        [FromBody] AILensInputDto inputDto,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var aiLens = await aiLensService.CreateAILensAsync(inputDto, userId, cancellationToken);

        return OkApiResponse(aiLens);
    }

    [HttpPut("{lensId}")]
    public async Task<IActionResult> UpdateAILens(
         Guid lensId,
         [FromBody] AILensInputDto inputDto,
         CancellationToken cancellationToken)
    {
        inputDto.Id = lensId;

        var userId = User.GetUserId();
        var aiLens = await aiLensService.UpdateAILensAsync(inputDto, userId, cancellationToken);

        return OkApiResponse(aiLens);
    }

    [HttpPut("me/{lensId}/enabled")]
    public async Task<IActionResult> SetIsUserLensEnabled(
       Guid lensId,
       [FromBody] SetIsUserBuiltInAILensEnabledInputDto inputDto,
       CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        await this.aiLensService.SetUserAILensEnabledAsync(
            lensId,
            userId,
            inputDto.IsEnabled,
            cancellationToken).NoSync();

        return Ok();
    }

    [HttpPost("name/exists")]
    public async Task<IActionResult> AILensWithSameNameExists(
        [FromBody] CheckAILensNameExistsInputDto inputDto,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var exists = await aiLensService.AILensWithSameNameExists(inputDto, userId, cancellationToken);

        if (exists)
        {
            return Ok("AI Lens with the same name already exists.");
        }

        return Ok(false);
    }
}
