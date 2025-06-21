namespace ZODs.Common.Exceptions;

public class BusinessValidationException : Exception
{
    public BusinessValidationException()
        : base()
    {
    }

    public BusinessValidationException(string message)
        : base(message)
    {
    }
}
