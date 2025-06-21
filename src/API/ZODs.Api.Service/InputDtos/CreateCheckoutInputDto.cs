using ZODs.Api.Repository.Entities.Enums;
using ZODs.Common.Attributes;
using System.ComponentModel.DataAnnotations;

namespace ZODs.Api.Service.InputDtos
{
    public sealed class CreateCheckoutInputDto
    {
        [Required]
        [ValidEnum]
        public PricingPlanType PricingPlanType { get; set; }

        [Required]
        [ValidEnum]
        public PricingPlanVariantType PricingPlanVariantType { get; set; }

        public string? PromoCode { get; set; }
    }
}