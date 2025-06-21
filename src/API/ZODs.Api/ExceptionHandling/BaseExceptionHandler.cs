namespace ZODs.Api.ExceptionHandling
{
    public class BaseExceptionHandler
    {
        private readonly AbstractExceptionHandler abstractExceptionHandler;

        public BaseExceptionHandler()
        {
            this.abstractExceptionHandler = new DuplicateEntityExceptionHandler();

            // Chain other exception handlers here
            this.abstractExceptionHandler
                                        .SetNext(new ClientSideExceptionHandler())
                                        .SetNext(new EntityNotFoundExceptionHandler())
                                        .SetNext(new RequiredFieldExceptionHandler())
                                        .SetNext(new DateRangeExceptionHandler())
                                        .SetNext(new ReadOnlyFieldExceptionHandler())
                                        .SetNext(new ForbiddenActionExceptionHandler())
                                        .SetNext(new BusinessValidationExceptionHandler())
                                        .SetNext(new ArgumentExceptionHandler())
                                        .SetNext(new UnauthorizedAccessExceptionHandler())
                                        .SetNext(new InsufficientAICreditsExceptionHandler());
        }

        public ErrorDetails Handle(Exception exception)
        {
            return this.abstractExceptionHandler.Handle(exception);
        }
    }
}
