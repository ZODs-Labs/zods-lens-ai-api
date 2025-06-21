using System.Text.Json.Serialization;

namespace ZODs.Payment.InputModels
{
    public class PaymentInputPayload<TData>
    {
        [JsonPropertyName("data")]
        public TData Data { get; set; } = default!;

        public PaymentInputPayload(TData data)
        {
            Data = data;
        }
    }
}