namespace ZODs.Common.Constants;

// Reference: https://docs.lemonsqueezy.com/api/subscriptions#attributes
public static class SubscriptionStatus
{
    /// <summary>
    /// Active subscription.
    /// </summary>
    public const string Active = "active";

    /// <summary>
    /// The subscription's payment collection has been paused. See the pause attribute below for more information.
    /// </summary>
    public const string Paused = "paused";

    /// <summary>
    /// A renewal payment has failed. The subscription will go through 4 payment retries over the course of 2 weeks. 
    /// If a retry is successful, the subscription's status changes back to active. 
    /// If all four retries are unsuccessful, the status is changed to unpaid.
    /// </summary>
    public const string PastDue = "past_due";

    /// <summary>
    /// Payment recovery has been unsuccessful in capturing a payment after 4 attempts. 
    /// If dunning is enabled in your store, your dunning rules now will determine if the subscription
    /// becomes expired after a certain period. If dunning is turned off, the status 
    /// remains unpaid (it is up to you to determine what this means for users of your product).
    /// </summary>
    public const string Unpaid = "unpaid";

    /// <summary>
    /// The customer or store owner has cancelled future payments, but the subscription is still 
    /// technically active and valid (on a "grace period"). 
    /// The ends_at value shows the date-time when the subscription is scheduled to expire.
    /// </summary>
    public const string Cancelled = "cancelled";

    /// <summary>
    /// The subscription has ended (either it had previously been cancelled and the grace 
    /// period created from its final payment has run out, or it was previously unpaid and the subscription 
    /// was not re-activated during dunning). 
    /// The ends_at value shows the date-time when the subscription expired. 
    /// Customers should no longer have access to paid features.
    /// </summary>
    public const string Expired = "expired";
}