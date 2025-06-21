using System.Text.Json.Serialization;

namespace ZODs.Payment.InputModels
{
    public sealed class SubscriptionPatchModel
    {
        public SubscriptionPatchDataModel Data { get; set; } = null!;

        public static SubscriptionPatchModel CreateCancelModel(
            string subscriptionId,
            bool cancalled)
             => new()
             {
                 Data = new SubscriptionPatchDataModel
                 {
                     Type = "subscription",
                     Id = subscriptionId,
                     Attributes = new SubscriptionPatchAttributeModel
                     {
                         IsCancelled = cancalled
                     }
                 }
             };
    }

    public sealed class SubscriptionPatchAttributeModel
    {
        [JsonPropertyName("cancelled")]
        public bool IsCancelled { get; set; }
    }

    public sealed class SubscriptionPatchDataModel
    {
        public string Type { get; set; } = null!;

        public string Id { get; set; } = null!;

        public SubscriptionPatchAttributeModel Attributes { get; set; } = null!;
    }
}