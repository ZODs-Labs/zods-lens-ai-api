namespace ZODs.Api.Repository.Entities;

public sealed class PaymentTransaction : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid UserPlanId { get; set; }
    public UserPricingPlan UserPlan { get; set; } = null!;

    public string TransactionId { get; set; } = null!;

    public decimal Amount { get; set; } = 0;

    public string Currency { get; set; } = null!;

    public string PaymentMethod { get; set; } = null!;

    public DateTime PaymentDate { get; set; }

    public bool IsSuccessful { get; set; }

    public string Status { get; set; } = null!;
}

