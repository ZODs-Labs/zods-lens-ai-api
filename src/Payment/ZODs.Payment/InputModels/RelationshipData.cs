using System.Text.Json.Serialization;

namespace ZODs.Payment.InputModels
{
    public sealed class Relationship
    {
        [JsonPropertyName("data")]
        public RelationshipData Data { get; set; } = null!;

        public Relationship(string type, string id) => Data = new RelationshipData(type, id);
    }

    public sealed class RelationshipData
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        public RelationshipData(string type, string id)
        {
            Type = type;
            Id = id;
        }
    }
}