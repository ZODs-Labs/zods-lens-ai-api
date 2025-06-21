using ZODs.Common.Exceptions;
using Serilog;

namespace ZODs.Api.ExceptionHandling
{
    public class ReadOnlyFieldExceptionHandler : AbstractExceptionHandler
    {
        public override ErrorDetails Handle(Exception exception)
        {
            if (exception is ReadOnlyFieldException)
            {
                Log.Error($"Read only field: {exception}");

                return new ErrorDetails()
                {
                    Message = string.IsNullOrEmpty(exception.Message) ? "Read only field" : exception.Message,
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
