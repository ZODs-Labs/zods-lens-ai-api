using System.ComponentModel.DataAnnotations;
using ZODs.Api.Repository.Entities.Enums;

namespace ZODs.Api.Repository.Entities;

public class Feature : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string Key { get; set; } = null!;

    public string Description { get; set; } = null!;

    public FeatureIndex FeatureIndex { get; set; }

    public ICollection<PricingPlanFeature> PlanFeatures { get; set; } = new List<PricingPlanFeature>();

    public ICollection<FeatureLimitation> Limitations { get; set; } = new List<FeatureLimitation>();

    public ICollection<UserFeature> UserFeatures { get; set; } = new List<UserFeature>();
}
