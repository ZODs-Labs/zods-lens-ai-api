using ZODs.Common.Exceptions;
using Serilog;

namespace ZODs.Api.ExceptionHandling
{
    public class DuplicateEntityExceptionHandler : AbstractExceptionHandler
    {
        public override ErrorDetails Handle(Exception exception)
        {
            if (exception is DuplicateEntityException)
            {
                Log.Error($"Entity already exists: {exception}");

                var message = exception.Message;
                if (string.IsNullOrWhiteSpace(message))
                {
                    message = "Entity already exists.";
                }

                return new ErrorDetails()
                {
                    Message = message,
                    StatusCode = StatusCodes.Status409Conflict,
                };
            }
            else
            {
                return base.Handle(exception);
            }
        }
    }
}
