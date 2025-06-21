namespace ZODs.AI.OpenAI.Interfaces;

public interface ITokenizerService
{
    int CountTokensFromString(string model, string input);
}