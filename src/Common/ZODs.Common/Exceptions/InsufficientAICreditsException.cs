namespace ZODs.Common.Exceptions;

public sealed class InsufficientAICreditsException : Exception
{
    public int CreditsBalance { get; set; }

    public InsufficientAICreditsException()
        : base()
    {
    }

    public InsufficientAICreditsException(string message)
        : base(message)
    {
    }

    public InsufficientAICreditsException(string message, int creditsBalance)
        : base(message)
    {
        CreditsBalance = creditsBalance;
    }
}