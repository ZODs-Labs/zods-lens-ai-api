using Serilog;

namespace ZODs.Api.ExceptionHandling
{
    public class EntityNotFoundExceptionHandler : AbstractExceptionHandler
    {
        public override ErrorDetails Handle(Exception exception)
        {
            if (exception is KeyNotFoundException)
            {
                Log.Error($"Entity not found: {exception}");

                return new ErrorDetails()
                {
                    Message = "Entity not found.",
                    StatusCode = StatusCodes.Status404NotFound,
                };
            }
            else
            {
                return base.Handle(exception);
            }
        }
    }
}
