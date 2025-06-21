using Azure.Core;
using Azure;
using Azure.Core.Pipeline;
using ZODs.AI.Common.Configuration;
using ZODs.AI.Common.Extensions;
using System.Text.Json;

namespace ZODs.AI.Common.Clients;

public abstract class AIHttpClient<TCompletion>
    where TCompletion : IAICompletion
{
    private readonly HttpClient httpClient;
    protected readonly HttpPipeline httpPipeline;
    protected Func<JsonElement, TCompletion> completionDeserializer;

    protected static ResponseClassifier ResponseClassifier200 => new StatusCodeClassifier([200]);

    private static readonly RequestContext DefaultRequestContext = new();
    private static readonly string[] AuthorizationScopes = [];

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public AIHttpClient(
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        HttpClient httpClient,
        string apiKey)
    {
        this.httpClient = httpClient;
        var tokenCredential = CreateDelegatedToken(apiKey);

        this.httpPipeline = HttpPipelineBuilder.Build(
            new AIClientOptions(),
            [],
            [
                new BearerTokenAuthenticationPolicy(
                    tokenCredential,
                    AuthorizationScopes)
            ],
            new ResponseClassifier());
    }

    protected virtual async Task<StreamingResponse<TCompletion>> GetCompletionsStreamingInternalAsync(
        IAIRequestOptions requestOptions,
        CancellationToken cancellationToken = default)
    {
        RequestContent content = requestOptions.ToRequestContent();
        RequestContext context = FromCancellationToken(cancellationToken);

        // Response value object takes IDisposable ownership of message
        HttpMessage message = CreatePostRequestMessage(requestOptions, content, context);
        message.BufferResponse = false;
        Response baseResponse = await httpPipeline.ProcessMessageAsync(message, context, cancellationToken)
            .ConfigureAwait(false);
        return StreamingResponse<TCompletion>.CreateFromResponse(
            baseResponse,
            (responseForEnumeration) => SseAsyncEnumerator<TCompletion>.EnumerateFromSseStream(
                responseForEnumeration.ContentStream!,
                completionDeserializer,
                cancellationToken));

    }

    protected abstract HttpMessage CreatePostRequestMessage<TAIRequestOptions>(TAIRequestOptions options, RequestContent content, RequestContext context)
        where TAIRequestOptions : IAIRequestOptions;

    internal static RequestContext FromCancellationToken(CancellationToken cancellationToken = default)
    {
        if (!cancellationToken.CanBeCanceled)
        {
            return DefaultRequestContext;
        }

        return new RequestContext() { CancellationToken = cancellationToken };
    }

    private static TokenCredential CreateDelegatedToken(string token)
    {
        var accessToken = new AccessToken(token, DateTimeOffset.Now.AddDays(180));
        return DelegatedTokenCredential.Create((_, _) => accessToken);
    }
}
