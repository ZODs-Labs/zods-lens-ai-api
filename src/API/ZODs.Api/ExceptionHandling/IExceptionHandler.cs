namespace ZODs.Api.ExceptionHandling
{
    /// <summary>
    /// The Handler interface declares a method for building the chain of handlers. It also declares a method for executing a request.
    /// </summary>
    public interface IExceptionHandler
    {
        /// <summary>
        /// Sets next handler.
        /// </summary>
        /// <param name="handler">Next handler.</param>
        /// <returns>Current handler.</returns>
        IExceptionHandler SetNext(IExceptionHandler handler);

        /// <summary>
        /// Handles exception.
        /// </summary>
        /// <param name="exception">Exception.</param>
        /// <returns>Error details.</returns>
        ErrorDetails Handle(Exception exception);
    }
}
