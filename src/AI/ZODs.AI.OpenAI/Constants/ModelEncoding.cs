namespace ZODs.AI.OpenAI.Constants;

public static class ModelEncoding
{
    private static readonly string AssemblyName = "ZODs.AI.OpenAI.Files";

    public static readonly IReadOnlyDictionary<string, string> MODEL_PREFIX_TO_ENCODING = new Dictionary<string, string>
        {
            { "gpt-4", "cl100k_base" },
            { "gpt-3.5-turbo-", "cl100k_base" }
        };

    public static readonly IReadOnlyDictionary<string, string> MODEL_TO_ENCODING = new Dictionary<string, string>
        {
            { "gpt-4", "cl100k_base" },
            { "gpt-3.5-turbo", "cl100k_base" },
            { "text-davinci-003", "p50k_base" },
            { "text-davinci-002", "p50k_base" },
            { "text-davinci-001", "r50k_base" },
            { "text-curie-001", "r50k_base" },
            { "text-babbage-001", "r50k_base" },
            { "text-ada-001", "r50k_base" },
            { "davinci", "r50k_base" },
            { "curie", "r50k_base" },
            { "babbage", "r50k_base" },
            { "ada", "r50k_base" },
            { "code-davinci-002", "p50k_base" },
            { "code-davinci-001", "p50k_base" },
            { "code-cushman-002", "p50k_base" },
            { "code-cushman-001", "p50k_base" },
            { "davinci-codex", "p50k_base" },
            { "cushman-codex", "p50k_base" },
            { "text-davinci-edit-001", "p50k_edit" },
            { "code-davinci-edit-001", "p50k_edit" },
            { "text-embedding-ada-002", "cl100k_base" },
            { "text-similarity-davinci-001", "r50k_base" },
            { "text-similarity-curie-001", "r50k_base" },
            { "text-similarity-babbage-001", "r50k_base" },
            { "text-similarity-ada-001", "r50k_base" },
            { "text-search-davinci-doc-001", "r50k_base" },
            { "text-search-curie-doc-001", "r50k_base" },
            { "text-search-babbage-doc-001", "r50k_base" },
            { "text-search-ada-doc-001", "r50k_base" },
            { "code-search-babbage-code-001", "r50k_base" },
            { "code-search-ada-code-001", "r50k_base" },
            { "gpt2", "gpt2" }
        };

    public static readonly IReadOnlyDictionary<string, string> ENCODING_TO_ENCODING_FILE = new Dictionary<string, string>
    {
        { "cl100k_base", $"{AssemblyName}.cl100k_base.tiktoken" },
        { "p50k_base", $"{AssemblyName}.p50k_base.tiktoken" },
        { "r50k_base", $"{AssemblyName}.r50k_base.tiktoken" },
        { "p50k_edit", $"{AssemblyName}.p50k_edit.tiktoken" },
    };
}