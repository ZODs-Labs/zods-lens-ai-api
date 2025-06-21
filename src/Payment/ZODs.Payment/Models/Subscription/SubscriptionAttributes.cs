using System.Text.Json.Serialization;

namespace ZODs.Payment.Models.Subscription;

public sealed class SubscriptionAttributes
{
    [JsonPropertyName("store_id")]
    public int? StoreId { get; set; }

    [JsonPropertyName("customer_id")]
    public int? CustomerId { get; set; }

    [JsonPropertyName("order_id")]
    public int? OrderId { get; set; }

    [JsonPropertyName("order_item_id")]
    public int? OrderItemId { get; set; }

    [JsonPropertyName("product_id")]
    public int? ProductId { get; set; }

    [JsonPropertyName("variant_id")]
    public int? VariantId { get; set; }

    [JsonPropertyName("product_name")]
    public string? ProductName { get; set; } = default!;

    [JsonPropertyName("variant_name")]
    public string? VariantName { get; set; } = default!;

    [JsonPropertyName("user_name")]
    public string? UserName { get; set; } = default!;

    [JsonPropertyName("user_email")]
    public string? UserEmail { get; set; } = default!;

    [JsonPropertyName("status")]
    public string? Status { get; set; } = default!;

    [JsonPropertyName("status_formatted")]
    public string? StatusFormatted { get; set; } = default!;

    [JsonPropertyName("card_brand")]
    public string? CardBrand { get; set; } = default!;

    [JsonPropertyName("card_last_four")]
    public string? CardLastFour { get; set; } = default!;

    [JsonPropertyName("pause")]
    public bool? Pause { get; set; }

    [JsonPropertyName("cancelled")]
    public bool? Cancelled { get; set; }

    [JsonPropertyName("trial_ends_at")]
    public DateTime? TrialEndsAt { get; set; }

    [JsonPropertyName("billing_anchor")]
    public int? BillingAnchor { get; set; }

    [JsonPropertyName("first_subscription_item")]
    public SubscriptionItem FirstSubscriptionItem { get; set; } = default!;

    [JsonPropertyName("urls")]
    public SubscriptionUrls Urls { get; set; } = default!;

    [JsonPropertyName("renews_at")]
    public DateTime? RenewsAt { get; set; }

    [JsonPropertyName("ends_at")]
    public DateTime? EndsAt { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("test_mode")]
    public bool TestMode { get; set; }
}