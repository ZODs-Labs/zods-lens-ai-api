namespace ZODs.Api.Repository.Entities;

public sealed class PricingPlanFeature : BaseEntity
{
    public Guid PricingPlanId { get; set; }
    public PricingPlan PricingPlan { get; set; } = null!;

    public Guid FeatureId { get; set; }
    public Feature Feature { get; set; } = null!;

    public ICollection<PricingPlanFeatureRole> PlanFeatureRoles { get; set; } = new List<PricingPlanFeatureRole>();
    public ICollection<PricingPlanFeatureLimitation> Limitations { get; set; } = new List<PricingPlanFeatureLimitation>();
}
