using ZODs.Api.Repository.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Repository.Entities.Entities;
using ZODs.Api.Repository.Entities.AI;

namespace ZODs.Api.Repository.Entities;

public class User : IdentityUser<Guid>
{
    [Required]
    [MaxLength(200)]
    [MinLength(1)]
    public string FirstName { get; set; } = null!;

    [Required]
    [MinLength(1)]
    [MaxLength(200)]
    public string LastName { get; set; } = null!;

    [MaxLength(5)]
    public string? CardLast4 { get; set; }

    [MaxLength(15)]
    public string? CardBrand { get; set; }

    [MaxLength(300)]
    public string? UpdatePaymentMethodUrl { get; set; }

    [Required]
    public UserRegistrationType RegistrationType { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual UserAICreditBalance UserAICreditBalance { get; set; } = null!;

    public ICollection<Snippet> UserSnippets { get; set; } = new List<Snippet>();

    public ICollection<WorkspaceMember> Workspaces { get; set; } = new List<WorkspaceMember>();

    public ICollection<AILens> AILenses { get; set; } = new List<AILens>();

    public ICollection<UserAILensSettings> UserAILensSettings { get; set; } = new List<UserAILensSettings>();

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();

    public ICollection<UserPricingPlan> UserPricingPlans { get; set; } = new List<UserPricingPlan>();

    public ICollection<SnippetTriggerPrefix> SnippetTriggerPrefixes { get; set; } = new List<SnippetTriggerPrefix>();

    public ICollection<UserFeature> UserFeatures { get; set; } = new List<UserFeature>();
}