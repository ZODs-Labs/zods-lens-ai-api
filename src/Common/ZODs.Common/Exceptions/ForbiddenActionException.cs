namespace ZODs.Common.Exceptions;

public class ForbiddenActionException : Exception
{
    public ForbiddenActionException()
        : base()
    {
    }

    public ForbiddenActionException(string message)
        : base(message)
    {
    }
}
