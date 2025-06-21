using ZODs.AI.Common;
using ZODs.AI.Google.Models;

namespace ZODs.AI.Google
{
    internal interface IGeminiAIClient
    {
        Task<StreamingResponse<GeminiAICompletion>> GetCompletionsStreamingAsync(GeminiAIRequestOptions requestOptions, CancellationToken cancellationToken = default);
    }
}
