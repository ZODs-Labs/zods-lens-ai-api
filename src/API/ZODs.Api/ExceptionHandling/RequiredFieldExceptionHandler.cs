using ZODs.Common.Exceptions;
using Serilog;

namespace ZODs.Api.ExceptionHandling
{
    public class RequiredFieldExceptionHandler : AbstractExceptionHandler
    {
        public override ErrorDetails Handle(Exception exception)
        {
            if (exception is RequiredFieldException)
            {
                Log.Error($"Required field: {exception}");

                return new ErrorDetails()
                {
                    Message = string.IsNullOrEmpty(exception.Message) ? "Required field" : exception.Message,
                    StatusCode = StatusCodes.Status400BadRequest,
                };
            }
            else
            {
                return base.Handle(exception);
            }
        }
    }
}
