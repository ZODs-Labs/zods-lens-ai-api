using System.Text.Json.Serialization;

namespace ZODs.Payment.InputModels
{
    public class PaymentInputData<TRelationships, TAttributes>
    {
        [JsonPropertyName("type")]
        public virtual string Type { get; set; } = null!;

        [JsonPropertyName("relationships")]
        public TRelationships? Relationships { get; set; }

        [JsonPropertyName("attributes")]
        public TAttributes Attributes { get; set; } = default!;

        public PaymentInputData(string type, TRelationships? relationships, TAttributes attributes)
        {
            Type = type;
            Relationships = relationships;
            Attributes = attributes;
        }
    }
}