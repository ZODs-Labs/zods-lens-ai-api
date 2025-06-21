namespace ZODs.AI.OpenAI.Utils;

public static class OpenAICreditsCalculator
{
    public static int ConvertOpenAITokensToCredits(int tokens)
        => (int)Math.Ceiling(tokens / 10M);

    public static int ConvertOpenAICreditsToTokens(int credits)
        => credits * 10;

    public static decimal CalculateGpt3CreditsPrice(int credits)
        => credits * 0.00003M;

    public static decimal CalculateGpt4CreditsPrice(int credits)
        => credits * 0.0008M;
}