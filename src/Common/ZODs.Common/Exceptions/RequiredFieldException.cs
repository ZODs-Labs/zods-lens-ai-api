namespace ZODs.Common.Exceptions;

public class RequiredFieldException : Exception
{
    public RequiredFieldException()
        : base()
    {
    }

    public RequiredFieldException(string message)
        : base(message)
    {
    }
}
