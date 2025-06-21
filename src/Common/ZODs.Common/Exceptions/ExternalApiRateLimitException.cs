namespace ZODs.Common.Exceptions;

public class ExternalApiRateLimitException : Exception
{
    public ExternalApiRateLimitException()
        : base()
    {
    }

    public ExternalApiRateLimitException(string message)
        : base(message)
    {
    }
}
