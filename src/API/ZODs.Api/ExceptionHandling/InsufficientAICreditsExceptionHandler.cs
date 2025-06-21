using Serilog;
using ZODs.Common.Exceptions;

namespace ZODs.Api.ExceptionHandling;

public class InsufficientAICreditsExceptionHandler : AbstractExceptionHandler
{
    public override ErrorDetails Handle(Exception exception)
    {
        if (exception is InsufficientAICreditsException ex)
        {
            Log.Error("Insufficient AI credits: {exception}", exception);

            return new ErrorDetails
            {
                Message = string.IsNullOrWhiteSpace(ex.Message) ? "Insufficient AI credits" : ex.Message,
                StatusCode = StatusCodes.Status400BadRequest
            };
        }
        else
        {
            return base.Handle(exception);
        }
    }
}
