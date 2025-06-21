using Azure;
using Azure.Core;
using Microsoft.Extensions.Options;
using ZODs.AI.Common;
using ZODs.AI.Common.Clients;

namespace ZODs.AI.Together;

public sealed class TogetherAIClient : AIHttpClient<TogetherAICompletion>, ITogetherAIClient
{
    private readonly Uri endpoint;

    public TogetherAIClient(
     HttpClient httpClient,
     IOptions<TogetherAIOptions> options)
        : base(httpClient, options.Value.ApiKey)
    {
        base.completionDeserializer = TogetherAICompletion.DeserializeCompletions;
        this.endpoint = new(options.Value.Endpoint);
    }

    public Task<StreamingResponse<TogetherAICompletion>> GetCompletionsStreamingAsync(
        TogetherAIRequestOptions requestOptions,
        CancellationToken cancellationToken = default)
    {
        requestOptions.StopSequences = new[] { "</s>", "[INST]" };
        return base.GetCompletionsStreamingInternalAsync(
            requestOptions,
            cancellationToken);
    }

    protected override HttpMessage CreatePostRequestMessage<TAIRequestOptions>(
        TAIRequestOptions options,
        RequestContent content,
        RequestContext context)
    {
        return CreatePostRequestMessage(content, context);
    }

    internal HttpMessage CreatePostRequestMessage(
        RequestContent content,
        RequestContext context)
    {
        HttpMessage message = httpPipeline.CreateMessage(context, ResponseClassifier200);
        Request request = message.Request;
        request.Method = RequestMethod.Post;
        request.Uri = GetUri();
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("Content-Type", "application/json");
        request.Content = content;
        return message;
    }

    internal RequestUriBuilder GetUri()
    {
        var uri = new RawRequestUriBuilder();
        uri.Reset(endpoint);
        return uri;
    }
}
