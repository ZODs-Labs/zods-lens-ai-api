using System.Text.Json;

namespace ZODs.AI.Common;

public interface IUtf8JsonSerializable
{
    void Write(Utf8JsonWriter writer);
}
