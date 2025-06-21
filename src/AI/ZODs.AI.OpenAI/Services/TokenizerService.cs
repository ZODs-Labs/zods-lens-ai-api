using Microsoft.DeepDev;
using ZODs.AI.OpenAI.Constants;
using ZODs.AI.OpenAI.Interfaces;
using System.Reflection;

namespace ZODs.AI.OpenAI.Services;

public sealed class TokenizerService : ITokenizerService
{
    private readonly Dictionary<string, ITokenizer> tokenizers = new();

    public int CountTokensFromString(string model, string input)
    {
        var tokenizer = GetTokenizerAsync(model);
        var encoded = tokenizer.Encode(input, new HashSet<string>());

        return encoded.Count;
    }

    private static string GetEncoderName(string modelName)
    {
        string? value = null;
        if (!ModelEncoding.MODEL_TO_ENCODING.TryGetValue(modelName, out value))
        {
            value = ModelEncoding.MODEL_PREFIX_TO_ENCODING.FirstOrDefault(x => modelName.StartsWith(x.Key)).Value;
        }

        if(string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"Model name '{modelName}' is not supported.", nameof(modelName));
        }

        return value;
    }

    private static TikTokenizer CreateByEncoderNameAsync(string encoderName, IReadOnlyDictionary<string, int>? extraSpecialTokens = null)
    {
        switch (encoderName)
        {
            case "cl100k_base":
                {
                    Dictionary<string, int> dictionary = new Dictionary<string, int>
                    {
                        { "<|endoftext|>", 100257 },
                        { "<|fim_prefix|>", 100258 },
                        { "<|fim_middle|>", 100259 },
                        { "<|fim_suffix|>", 100260 },
                        { "<|endofprompt|>", 100276 }
                    };
                    if (extraSpecialTokens != null)
                    {
                        dictionary = dictionary.Concat(extraSpecialTokens).ToDictionary((KeyValuePair<string, int> pair) => pair.Key, (KeyValuePair<string, int> pair) => pair.Value);
                    }

                    var regexPatternStr = "(?i:'s|'t|'re|'ve|'m|'ll|'d)|[^\\r\\n\\p{L}\\p{N}]?\\p{L}+|\\p{N}{1,3}| ?[^\\s\\p{L}\\p{N}]+[\\r\\n]*|\\s*[\\r\\n]+|\\s+(?!\\S)|\\s+";

                    return CreateTokenizer("cl100k_base", dictionary, regexPatternStr);
                }
            case "p50k_base":
                {
                    Dictionary<string, int> dictionary = new() { { "<|endoftext|>", 50256 } };
                    if (extraSpecialTokens != null)
                    {
                        dictionary = dictionary.Concat(extraSpecialTokens).ToDictionary((KeyValuePair<string, int> pair) => pair.Key, (KeyValuePair<string, int> pair) => pair.Value);
                    }

                    var regexPatternStr = "'s|'t|'re|'ve|'m|'ll|'d| ?\\p{L}+| ?\\p{N}+| ?[^\\s\\p{L}\\p{N}]+|\\s+(?!\\S)|\\s+";

                    return  CreateTokenizer("p50k_base", dictionary, regexPatternStr);
                }
            case "p50k_edit":
                {
                    Dictionary<string, int> dictionary = new()
                    {
                    { "<|endoftext|>", 50256 },
                    { "<|fim_prefix|>", 50281 },
                    { "<|fim_middle|>", 50282 },
                    { "<|fim_suffix|>", 50283 }
                };
                    if (extraSpecialTokens != null)
                    {
                        dictionary = dictionary.Concat(extraSpecialTokens).ToDictionary((KeyValuePair<string, int> pair) => pair.Key, (KeyValuePair<string, int> pair) => pair.Value);
                    }

                    var regexPatternStr = "'s|'t|'re|'ve|'m|'ll|'d| ?\\p{L}+| ?\\p{N}+| ?[^\\s\\p{L}\\p{N}]+|\\s+(?!\\S)|\\s+";

                    return  CreateTokenizer("p50k_edit", dictionary, regexPatternStr);
                }
            case "r50k_base":
                {
                    Dictionary<string, int> dictionary = new() { { "<|endoftext|>", 50256 } };
                    if (extraSpecialTokens != null)
                    {
                        dictionary = dictionary.Concat(extraSpecialTokens).ToDictionary((KeyValuePair<string, int> pair) => pair.Key, (KeyValuePair<string, int> pair) => pair.Value);
                    }

                    var regexPatternStr = "'s|'t|'re|'ve|'m|'ll|'d| ?\\p{L}+| ?\\p{N}+| ?[^\\s\\p{L}\\p{N}]+|\\s+(?!\\S)|\\s+";

                    return CreateTokenizer("r50k_base", dictionary, regexPatternStr);
                }
            default:
                throw new NotImplementedException("Doesn't support this encoder [" + encoderName + "]");
        }
    }

    private static TikTokenizer CreateTokenizer(string encoding, IReadOnlyDictionary<string, int> specialTokensEncoder, string pattern)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        if (!ModelEncoding.ENCODING_TO_ENCODING_FILE.TryGetValue(encoding, out var encodingFilePath))
        {
            throw new NotSupportedException("Doesn't support this encoding [" + encoding + "]");
        }

        using var stream = assembly.GetManifestResourceStream(encodingFilePath);
        if (stream == null)
        {
            throw new ArgumentException($"Tiktoken file for '{encoding}' encoding not found. Ensure the resource name is correct and the file has been set as an Embedded Resource.", nameof(encoding));
        }

        return new TikTokenizer(stream, specialTokensEncoder, pattern, 8192);
    }

    private ITokenizer GetTokenizerAsync(string modelName, IReadOnlyDictionary<string, int>? extraSpecialTokens = null)
    {
        var encoderName = GetEncoderName(modelName);
        if (!tokenizers.TryGetValue(encoderName, out ITokenizer? value))
        {
            value = CreateByEncoderNameAsync(encoderName, extraSpecialTokens);
            tokenizers.Add(encoderName, value);
        }

        return value;
    }
}