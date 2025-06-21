using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace ZODs.AI.OpenAI.Extensions;

public static class HttpResponseExtensions
{
    // Extension method to stream chat responses
    public static async Task<string> StreamChatCompletionsAsync(
        this HttpResponse response,
        StreamingResponse<StreamingChatCompletionsUpdate> streamingResponse,
        bool bufferContent = true,
        CancellationToken cancellationToken = default)
    {
        response.ContentType = "text/event-stream";
        // Disable buffering and timeout for the request
        response.Headers.Add("Cache-Control", "no-cache");
        response.Headers.Add("X-Accel-Buffering", "no");

        CompletionsFinishReason? finishReason = default;

        var bufferBuilder = new StringBuilder();

        await using var writer = new StreamWriter(response.Body);
        await foreach (var chatUpdate in streamingResponse)
        {
            // Stream each message within the chat update (choice)
            // Write the content to the response stream
            await writer.WriteAsync(chatUpdate.ContentUpdate);

            if (bufferContent)
            {
                bufferBuilder.Append(chatUpdate.ContentUpdate);
            }

            // Flush the stream to send the content to the client immediately
            await writer.FlushAsync(cancellationToken);

            finishReason = chatUpdate.FinishReason;
        }

        if (finishReason == CompletionsFinishReason.TokenLimitReached)
        {
            await writer.WriteAsync("The maximum number of tokens was reached.");
        }

        return bufferBuilder.ToString();
    }
}