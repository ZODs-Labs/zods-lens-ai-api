using ZODs.Common.Exceptions;
using Serilog;

namespace ZODs.Api.ExceptionHandling
{
    public class DateRangeExceptionHandler : AbstractExceptionHandler
    {
        public override ErrorDetails Handle(Exception exception)
        {
            if (exception is DateRangeException)
            {
                Log.Error($"Date range is not valid: {exception}");

                return new ErrorDetails()
                {
                    Message = string.IsNullOrEmpty(exception.Message) ? "Date range is not valid" : exception.Message,
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
