namespace ZODs.Common.Exceptions;

public sealed class ClientSideException : Exception
{
    public ClientSideException()
        : base()
    {
    }

    public ClientSideException(string message)
        : base(message)
    {
    }
}