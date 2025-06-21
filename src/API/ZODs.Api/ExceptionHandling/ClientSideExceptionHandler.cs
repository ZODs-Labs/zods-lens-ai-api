using Serilog;
using ZODs.Common.Exceptions;

namespace ZODs.Api.ExceptionHandling;

public sealed class ClientSideExceptionHandler: AbstractExceptionHandler
{
    public override ErrorDetails Handle(Exception exception)
    {
        if (exception is ClientSideException)
        {
            Log.Error($"Client side exception: {exception}");

            return new ErrorDetails()
            {
                Message = string.IsNullOrEmpty(exception.Message) ? "Unable to process request due to client side caused problem." : exception.Message,
                StatusCode = StatusCodes.Status400BadRequest,
            };
        }
        else
        {
            return base.Handle(exception);
        }
    }
}
