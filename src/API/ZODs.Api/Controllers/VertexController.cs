using Google.Apis.Auth.OAuth2;
using Google.Cloud.AIPlatform.V1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZODs.Api.Common.Attributes;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ZODs.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class VertexController : BaseController
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly PredictionServiceClient client;

    public VertexController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        this.client = new PredictionServiceClientBuilder
        {
            Endpoint = "us-central1-aiplatform.googleapis.com",
        }.Build();
    }

    [NonAction]
    [AllowAnonymous]
    [AllowNoSubscription]
    [HttpGet("stream-response")]
    public async Task StreamResponseAsync(
        [FromQuery] string prompt,
        CancellationToken cancellationToken)
    {
        string url = "https://us-central1-aiplatform.googleapis.com/v1/projects/codemanage-62d04/locations/us-central1/publishers/google/models/codechat-bison:serverStreamingPredict";
        string bearerToken = "";

        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        // Construct the JSON body for the POST request
        var requestBody = new
        {
            inputs = new[]
            {
                new
                {
                    struct_val = new
                    {
                        messages = new
                        {
                            list_val = new[]
                            {
                                new
                                {
                                    struct_val = new
                                    {
                                        content = new
                                        {
                                            string_val = new[] { prompt }
                                        },
                                        author = new
                                        {
                                            string_val = new[] { "User" }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            },
            parameters = new
            {
                struct_val = new
                {
                    temperature = new { float_val = 0.5 },
                    maxOutputTokens = new { int_val = 1024 },
                    topK = new { int_val = 40 },
                    topP = new { float_val = 0.95 }
                }
            }
        };

        var jsonString = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

        // Make the POST request to the external streaming service
        using var response = await httpClient.PostAsync(url, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        // Prepare the response for streaming
        Response.ContentType = "text/plain";

        // Stream the response content directly to the Response.Body
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);
        await using var writer = new StreamWriter(Response.Body);

        try
        {
            // StringBuilder to accumulate partial JSON objects
            StringBuilder jsonAccumulator = new StringBuilder();

            // Read and stream the response in chunks
            char[] buffer = new char[4096];
            int read;
            while ((read = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                jsonAccumulator.Append(buffer, 0, read);
                string xxx = jsonAccumulator.ToString();

                await writer.WriteAsync(buffer, 0, read);
                await writer.FlushAsync();
            }
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            // Log the exception
            // Decide if you want to write anything to the response or just close the connection
        }
    }
}
