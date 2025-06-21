using ZODs.Api.Repository.Entities.Enums;
using ZODs.Common.Attributes;
using System.ComponentModel.DataAnnotations;

namespace ZODs.Api.Service.Dtos.User;

public sealed class SignUpDto
{
    [Required]
    [MaxLength(200)]
    [MinLength(1)]
    public string FirstName { get; set; } = null!;

    [Required]
    [MinLength(1)]
    [MaxLength(200)]
    public string LastName { get; set; } = null!;

    [Required]
    [EmailAddress]
    [MaxLength(300)]
    public string Email { get; set; } = null!;

    [Required]
    [ValidPasswordComplexity]
    public string Password { get; set; } = null!;

    [ValidEnum]
    [Required]
    public PricingPlanType PricingPlanType { get; set; }
}
