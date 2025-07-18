﻿using System.Runtime.CompilerServices;
using System.Text.Json;

namespace ZODs.AI.Common;

internal static class SseAsyncEnumerator<T>
    where T : IAICompletion
{
    internal static async IAsyncEnumerable<T> EnumerateFromSseStreamCompletions(
        Stream stream,
        Func<JsonElement, IEnumerable<T>> multiElementDeserializer,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        try
        {
            using SseReader sseReader = new(stream);
            while (!cancellationToken.IsCancellationRequested)
            {
                SseLine? sseEvent = await sseReader.TryReadSingleFieldEventAsync().ConfigureAwait(false);
                if (sseEvent is not null)
                {
                    ReadOnlyMemory<char> name = sseEvent.Value.FieldName;
                    if (!name.Span.SequenceEqual("data".AsSpan()))
                    {
                        break;
                    }

                    ReadOnlyMemory<char> value = sseEvent.Value.FieldValue;
                    if (value.Span.SequenceEqual("[DONE]".AsSpan()))
                    {
                        break;
                    }
                    using JsonDocument sseMessageJson = JsonDocument.Parse(value);
                    IEnumerable<T> newItems = multiElementDeserializer.Invoke(sseMessageJson.RootElement);
                    foreach (T item in newItems)
                    {
                        yield return item;
                    }
                }
            }
        }
        finally
        {
            // Always dispose the stream immediately once enumeration is complete for any reason
            stream.Dispose();
        }
    }

    internal static IAsyncEnumerable<T> EnumerateFromSseStream(
        Stream stream,
        Func<JsonElement, T> elementDeserializer,
        CancellationToken cancellationToken = default)
        => EnumerateFromSseStreamCompletions(
            stream,
            (element) => new T[] { elementDeserializer.Invoke(element) },
            cancellationToken);
}
