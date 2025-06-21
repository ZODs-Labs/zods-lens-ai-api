using ZODs.AI.Common;

namespace ZODs.AI.Together
{
    public interface ITogetherAIClient
    {
        Task<StreamingResponse<TogetherAICompletion>> GetCompletionsStreamingAsync(TogetherAIRequestOptions requestOptions, CancellationToken cancellationToken = default);
    }
}
