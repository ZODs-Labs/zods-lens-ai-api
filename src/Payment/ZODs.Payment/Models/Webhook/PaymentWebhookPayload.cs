using System.Text.Json.Serialization;

namespace ZODs.Payment.Models.Webhook
{
    public class PaymentWebhookPayload<TAttributes>: PaymentApiPayload<PaymentData<TAttributes>>
    {
        [JsonPropertyName("meta")]
        public PaymentWebhookMeta Meta { get; set; } = new PaymentWebhookMeta();
    }
}