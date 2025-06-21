using ZODs.Common.Exceptions;
using Serilog;

namespace ZODs.Api.ExceptionHandling
{
    public class ForbiddenActionExceptionHandler : AbstractExceptionHandler
    {
        public override ErrorDetails Handle(Exception exception)
        {
            if (exception is ForbiddenActionException)
            {
                Log.Error($"Forbidden action: {exception}");

                return new ErrorDetails()
                {
                    Message = string.IsNullOrEmpty(exception.Message) ? "Forbidden action" : exception.Message,
                    StatusCode = StatusCodes.Status403Forbidden,
                };
            }
            else
            {
                return base.Handle(exception);
            }
        }
    }
}
