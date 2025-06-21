namespace ZODs.Common.Exceptions;

public class ReadOnlyFieldException : Exception
{
    public ReadOnlyFieldException()
        : base()
    {
    }

    public ReadOnlyFieldException(string message)
        : base(message)
    {
    }
}
