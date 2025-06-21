namespace ZODs.Api.Repository.Entities;

public sealed class PricingPlanFeatureRole
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public Guid PlanFeatureId { get; set; }
    public PricingPlanFeature PlanFeature { get; set; } = null!;
}

