namespace ZODs.Common.Exceptions;

public sealed class PaymentException : Exception
{
    public PaymentException()
        : base()
    {
    }

    public PaymentException(string message)
        : base(message)
    {
    }
}