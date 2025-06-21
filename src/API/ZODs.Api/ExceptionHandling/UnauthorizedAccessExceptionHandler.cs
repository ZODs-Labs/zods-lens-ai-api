using Serilog;

namespace ZODs.Api.ExceptionHandling
{
    public class UnauthorizedAccessExceptionHandler : AbstractExceptionHandler
    {
        public override ErrorDetails Handle(Exception exception)
        {
            if (exception is UnauthorizedAccessException)
            {
                Log.Error($"Unauthorized access: {exception}");

                return new ErrorDetails()
                {
                    Message = string.IsNullOrEmpty(exception.Message) ? "Unauthorized access." : exception.Message,
                    StatusCode = StatusCodes.Status401Unauthorized,
                };
            }
            else
            {
                return base.Handle(exception);
            }
        }
    }
}
