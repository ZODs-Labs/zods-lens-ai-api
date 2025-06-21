using Azure;
using Azure.Core;
using Microsoft.Extensions.Options;
using ZODs.AI.Common;
using ZODs.AI.Common.Clients;
using ZODs.AI.Google.Configuration;
using ZODs.AI.Google.Models;

namespace ZODs.AI.Google;

public sealed class GeminiAIClient
    : AIHttpClient<GeminiAICompletion>, IGeminiAIClient
{
    private readonly Uri endpoint;

    public GeminiAIClient(
        HttpClient httpClient,
        IOptions<GoogleAIOptions> options)
        : base(httpClient, options.Value.ApiKey)
    {
        base.completionDeserializer = GeminiAICompletion.DeserializeCompletions;
        endpoint = new(options.Value.Endpoint);
    }

    public Task<StreamingResponse<GeminiAICompletion>> GetCompletionsStreamingAsync(
        GeminiAIRequestOptions requestOptions,
        CancellationToken cancellationToken = default)
    {
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
