using System.ComponentModel.DataAnnotations;
using ZODs.Api.Repository.Entities.Enums;

namespace ZODs.Api.Repository.Entities;

public sealed class PricingPlan : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    [Required]
    [MinLength(10)]
    [MaxLength(300)]
    public string Description { get; set; } = null!;

    [Required]
    public PricingPlanType Type { get; set; }

    public ICollection<PricingPlanFeature> PlanFeatures { get; set; } = new List<PricingPlanFeature>();

    public ICollection<PricingPlanVariant> PricingPlanVariants { get; set; } = new List<PricingPlanVariant>();
}

