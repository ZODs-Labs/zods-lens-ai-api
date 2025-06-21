using Serilog;

namespace ZODs.Api.ExceptionHandling
{
    public class ArgumentExceptionHandler : AbstractExceptionHandler
    {
        public override ErrorDetails Handle(Exception exception)
        {
            if (exception is ArgumentException argumentException)
            {
                Log.Error($"Passed argument does not meet the parameter specification: {argumentException}");

                return new ErrorDetails()
                       {
                               Message = string.IsNullOrEmpty(exception.Message)
                                                 ? $"Passed argument does not meet the parameter specification: {argumentException.ParamName}"
                                                 : exception.Message,
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
