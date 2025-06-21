using ZODs.Common.Exceptions;
using Serilog;

namespace ZODs.Api.ExceptionHandling
{
    public class BusinessValidationExceptionHandler : AbstractExceptionHandler
    {
        public override ErrorDetails Handle(Exception exception)
        {
            if (exception is BusinessValidationException)
            {
                Log.Error($"Unprocessable entity: {exception}");

                return new ErrorDetails()
                {
                    Message = string.IsNullOrEmpty(exception.Message) ? "Unprocessable entity" : exception.Message,
                    StatusCode = StatusCodes.Status422UnprocessableEntity,
                };
            }
            else
            {
                return base.Handle(exception);
            }
        }
    }
}
