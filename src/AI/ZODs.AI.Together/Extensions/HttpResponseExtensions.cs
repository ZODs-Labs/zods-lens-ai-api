using Microsoft.AspNetCore.Http;
using ZODs.AI.Common;
using System.Text;

namespace ZODs.AI.Together;

public static class HttpResponseExtensions
{
    // Extension method to stream chat responses
    public static async Task<string> StreamChatCompletionsAsync(
        this HttpResponse response,
        StreamingResponse<TogetherAICompletion> streamingResponse,
        bool bufferContent = true,
        CancellationToken cancellationToken = default)
    {
        response.ContentType = "text/event-stream";
        // Disable buffering and timeout for the request
        response.Headers.Add("Cache-Control", "no-cache");
        response.Headers.Add("X-Accel-Buffering", "no");

        var bufferBuilder = new StringBuilder();

        await using var writer = new StreamWriter(response.Body);
        await foreach (var chatUpdate in streamingResponse)
        {
            // Stream each message within the chat update (choice)
            // Write the content to the response stream
            var text = chatUpdate.Choices[0].Text;
            await writer.WriteAsync(text);

            if (bufferContent)
            {
                bufferBuilder.Append(text);
            }

            // Flush the stream to send the content to the client immediately
            await writer.FlushAsync(cancellationToken);
        }

        return bufferBuilder.ToString();
    }
}
