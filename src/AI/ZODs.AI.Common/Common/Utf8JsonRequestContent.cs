﻿using Azure.Core;
using System.Text.Json;

namespace ZODs.AI.Common;

public class Utf8JsonRequestContent : RequestContent
{
    private readonly MemoryStream _stream;
    private readonly RequestContent _content;

    public Utf8JsonWriter JsonWriter { get; }

    public Utf8JsonRequestContent()
    {
        _stream = new MemoryStream();
        _content = Create(_stream);
        JsonWriter = new Utf8JsonWriter(_stream);
    }

    public override async Task WriteToAsync(Stream stream, CancellationToken cancellation)
    {
        await JsonWriter.FlushAsync(cancellation).ConfigureAwait(false);
        await _content.WriteToAsync(stream, cancellation).ConfigureAwait(false);
    }

    public override void WriteTo(Stream stream, CancellationToken cancellation)
    {
        JsonWriter.Flush();
        _content.WriteTo(stream, cancellation);
    }

    public override bool TryComputeLength(out long length)
    {
        length = JsonWriter.BytesCommitted + JsonWriter.BytesPending;
        return true;
    }

    public override void Dispose()
    {
        JsonWriter.Dispose();
        _content.Dispose();
        _stream.Dispose();
    }
}
