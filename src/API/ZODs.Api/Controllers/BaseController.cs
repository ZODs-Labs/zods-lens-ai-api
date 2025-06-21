using Microsoft.AspNetCore.Mvc;
using ZODs.Common.Models;

namespace ZODs.Api.Controllers;

[ApiController]
public class BaseController : ControllerBase
{
    protected IActionResult OkApiResponse<TModel>(TModel model)
    {
        return Ok(ApiResponse.Create(model));
    }

    protected async Task HandleInvalidModelState(CancellationToken cancellationToken)
    {
        // If the model is not valid, send a 400 Bad Request response
        // with the validation errors before any streaming starts.
        Response.StatusCode = StatusCodes.Status400BadRequest;
        var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
        await Response.WriteAsJsonAsync(new { message = string.Join(" ", errors) }, cancellationToken);

        return;
    }
}
