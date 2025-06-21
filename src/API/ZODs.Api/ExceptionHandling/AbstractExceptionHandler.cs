using Serilog;

namespace ZODs.Api.ExceptionHandling
{
    public abstract class AbstractExceptionHandler : IExceptionHandler
    {
        private IExceptionHandler nextHandler;

        public IExceptionHandler SetNext(IExceptionHandler handler)
        {
            this.nextHandler = handler;
            return handler;
        }

        public virtual ErrorDetails Handle(Exception exception)
        {
            if (this.nextHandler != null)
            {
                return this.nextHandler.Handle(exception);
            }
            else
            {
                Log.Error($"Internal Server Error {exception}");

                return new ErrorDetails()
                {
                    Message = "Internal Server Error.",
                    StatusCode = StatusCodes.Status500InternalServerError,
                };
            }
        }
    }
}
