using ZODs.Api.Common.Constants;
using ZODs.Api.Repository.Entities;
using ZODs.Api.Repository.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using ZODs.Api.Repository.Helpers;
using Microsoft.Extensions.Hosting;

namespace ZODs.Api.Repository;

public sealed class DataSeedRepository : IDataSeedRepository
{
    private readonly ZodsContext dbContext;
    private readonly IHostingEnvironment hostingEnvironment;

    public DataSeedRepository(
        ZodsContext dbContext,
        IHostingEnvironment hostingEnvironment)
    {
        this.dbContext = dbContext;
        this.hostingEnvironment = hostingEnvironment;

        PricingPlans = new List<PricingPlan>();
        PricingPlanVariants = new List<PricingPlanVariant>();
        Features = new List<Feature>();
        PricingPlanFeatures = new List<PricingPlanFeature>();
        FeatureLimitations = new List<FeatureLimitation>();
        PricingPlanFeatureLimitations = new List<PricingPlanFeatureLimitation>();
    }

    ICollection<PricingPlan> PricingPlans;
    ICollection<PricingPlanVariant> PricingPlanVariants;
    ICollection<Feature> Features;
    ICollection<PricingPlanFeature> PricingPlanFeatures;
    ICollection<FeatureLimitation> FeatureLimitations;
    ICollection<PricingPlanFeatureLimitation> PricingPlanFeatureLimitations;

    public async Task SeedAsync()
    {
        await SeedWorkspaceRoles();
        await SeedPricingPlans();
        await SeedPricingPlanVariants();
        await SeedFeatures();
        await SeedFeatureLimitations();
        await SeedPricingPlanFeatures();
        await SeedPricingPlanFeatureLimitations();
    }

    private async Task SeedWorkspaceRoles()
    {
        if (await dbContext.WorkspaceRoles.AnyAsync())
        {
            return;
        }

        var workspaceRoles = new WorkspaceRole[]
        {
            new WorkspaceRole
            {
                Name = "Owner",
                Description = "The owner of the workspace.",
                Index = WorkspaceMemberRoleIndex.Owner,
            },
            new WorkspaceRole
            {
                Name = "Admin",
                Description = "The admin of the workspace.",
                Index = WorkspaceMemberRoleIndex.Admin,
            },
            new WorkspaceRole
            {
                Name = "Member",
                Description = "The member of the workspace.",
                Index = WorkspaceMemberRoleIndex.Member,
            },
        };

        await dbContext.WorkspaceRoles.AddRangeAsync(workspaceRoles);
        await dbContext.SaveChangesAsync();
    }

    private async Task SeedPricingPlans()
    {
        if (await dbContext.PricingPlans.AnyAsync())
        {
            PricingPlans = await dbContext.PricingPlans.AsNoTracking().ToListAsync();
            return;
        }

        PricingPlans = new PricingPlan[]
        {

            new PricingPlan
            {
                Name = "Free",
                Type = PricingPlanType.Free,
                Description = "Free pricing plan with limited features.",
            },
            new PricingPlan
            {
                Name = "Basic",
                Type = PricingPlanType.Basic,
                Description = "Kickstart your coding with 300 personal snippets. Ideal for individual coders who value organization and simplicity.",
            },
            new PricingPlan
            {
                Name = "Standard",
                Type = PricingPlanType.Standard,
                Description = "Elevate your efficiency! Sync snippets across editors, collaborate in 3 workspaces, and enjoy the added flexibility of custom prefixes. Our top pick for collaborative coding!",
            },
            new PricingPlan
            {
                Name = "Premium",
                Type = PricingPlanType.Premium,
                Description = "Unlock limitless coding potential. Enjoy boundless snippets, extensive collaboration in 10 workspaces, and premium customization. Tailored for the ultimate coding connoisseur.",
            },
        };

        await dbContext.PricingPlans.AddRangeAsync(PricingPlans);
        await dbContext.SaveChangesAsync();
    }

    private async Task SeedPricingPlanVariants()
    {
        if (await dbContext.PricingPlanVariants.AnyAsync())
        {
            PricingPlanVariants = await dbContext.PricingPlanVariants.AsNoTracking().ToListAsync();
            return;
        }

        var freePricingPlanId = GetPricingPlanId(PricingPlanType.Free);
        var basicPricingPlanId = GetPricingPlanId(PricingPlanType.Basic);
        var standardPricingPlanId = GetPricingPlanId(PricingPlanType.Standard);
        var premiumPricingPlanId = GetPricingPlanId(PricingPlanType.Premium);

        PricingPlanVariants = new PricingPlanVariant[]
        {
            // Free Plan Variants
            new PricingPlanVariant
            {
                PricingPlanId = freePricingPlanId,
                VariantType = PricingPlanVariantType.Free,
                Price = 0M,
                VariantId = PricingPlanVariantHelper.GetPricingPlanVariantId(
                    PricingPlanType.Free,
                    PricingPlanVariantType.Free,
                    isProduction: hostingEnvironment.IsProduction()),
            },

            // Basic Plan Variants
            new PricingPlanVariant
            {
                PricingPlanId = basicPricingPlanId,
                VariantType = PricingPlanVariantType.Monthly,
                Price = 11.99M,
                VariantId = PricingPlanVariantHelper.GetPricingPlanVariantId(
                    PricingPlanType.Basic,
                    PricingPlanVariantType.Monthly,
                    isProduction: hostingEnvironment.IsProduction()),
            },
            new PricingPlanVariant
            {
                PricingPlanId = basicPricingPlanId,
                VariantType = PricingPlanVariantType.Yearly,
                Price = 119.99M,
                VariantId = PricingPlanVariantHelper.GetPricingPlanVariantId(
                    PricingPlanType.Basic,
                    PricingPlanVariantType.Yearly,
                    isProduction: hostingEnvironment.IsProduction()),
            },

            // Standard Plan Variants
            new PricingPlanVariant
            {
                PricingPlanId = standardPricingPlanId,
                VariantType = PricingPlanVariantType.Monthly,
                Price = 19.99M,
                VariantId = PricingPlanVariantHelper.GetPricingPlanVariantId(
                    PricingPlanType.Standard,
                    PricingPlanVariantType.Monthly,
                    isProduction: hostingEnvironment.IsProduction()),
            },
            new PricingPlanVariant
            {
                PricingPlanId = standardPricingPlanId,
                VariantType = PricingPlanVariantType.Yearly,
                Price = 199.99M,
                VariantId = PricingPlanVariantHelper.GetPricingPlanVariantId(
                    PricingPlanType.Standard,
                    PricingPlanVariantType.Yearly,
                    isProduction: hostingEnvironment.IsProduction()),
            },

            // Premium Plan Variants
            new PricingPlanVariant
            {
                PricingPlanId = premiumPricingPlanId,
                VariantType = PricingPlanVariantType.Monthly,
                Price = 39.99M,
                VariantId = PricingPlanVariantHelper.GetPricingPlanVariantId(
                    PricingPlanType.Premium,
                    PricingPlanVariantType.Monthly,
                    isProduction: hostingEnvironment.IsProduction()),
            },
            new PricingPlanVariant
            {
                PricingPlanId = premiumPricingPlanId,
                VariantType = PricingPlanVariantType.Yearly,
                Price = 399.99M,
                VariantId = PricingPlanVariantHelper.GetPricingPlanVariantId(
                    PricingPlanType.Premium,
                    PricingPlanVariantType.Yearly,
                    isProduction: hostingEnvironment.IsProduction()),
            },
        };

        await dbContext.PricingPlanVariants.AddRangeAsync(PricingPlanVariants);
        await dbContext.SaveChangesAsync();
    }

    private async Task SeedFeatures()
    {
        var featuresFromDb = await dbContext.Features.AsNoTracking().ToListAsync();

        Features = new Feature[]
        {
            // Pricing Plan Features
            new Feature
            {
                Name = "Personal Snippets",
                Key = FeatureKeys.PersonalSnippets,
                Description = "Allows the user to create snippets.",
                FeatureIndex = FeatureIndex.PersonalSnippets,
            },
            new Feature
            {
                Name = "Personal Snippet Prefixes",
                Key = FeatureKeys.PersonalSnippetPrefixes,
                Description = "Allows the user to create personal snippet prefixes.",
                FeatureIndex = FeatureIndex.PersonalSnippetPrefixes,
            },
            new Feature
            {
                Name = "Workspaces",
                Key = FeatureKeys.Workspaces,
                Description = "Allows the user to create workspaces.",
                FeatureIndex = FeatureIndex.Workspaces,
            },
            new Feature
            {
                Name = "Workspace Snippet Prefixes",
                Key = FeatureKeys.WorkspaceSnippetPrefixes,
                Description = "Allows the user to create workspace snippet prefixes.",
                FeatureIndex = FeatureIndex.WorkspaceSnippetPrefixes,
            },
            new Feature
            {
                Name = "Auto Snippets Sync",
                Key = FeatureKeys.AutoSnippetsSync,
                Description = "Automatically syncs snippets code editors.",
                FeatureIndex = FeatureIndex.AutoSnippetsSync,
            },
            new Feature
            {
                Name = "Workspace Scoped Snippet Environment",
                Key = FeatureKeys.WorkspaceScopedSnippetEnvironment,
                Description = "Ability to restrict workspace snippet to be visible only to the specified workspace within code editors.",
                FeatureIndex = FeatureIndex.WorkspaceScopedSnippetsEnvironment,
            },

            // AI Features
            new Feature
            {
                Name = "AI: GPT-3",
                Key = FeatureKeys.AIGpt3,
                Description = "Allows access to GPT-3 related AI features.",
                FeatureIndex = FeatureIndex.AIGpt3,
            },
            new Feature
            {
                Name = "AI: GPT-4",
                Key = FeatureKeys.AIGpt4,
                Description = "Allows access to GPT-4 related AI features.",
                FeatureIndex = FeatureIndex.AIGpt4,
            },
            new Feature
            {
                Name = "AI Lens",
                Key = FeatureKeys.AILens,
                Description = "Creating and using AI lenses.",
                FeatureIndex = FeatureIndex.AILens,
            },
            new Feature
            {
                Name = "Custom AI Api Keys",
                Key = FeatureKeys.CustomAiApiKeys,
                Description = "Ability to use custom AI api keys.",
                FeatureIndex = FeatureIndex.CustomAiApiKeys,
            },

            // Individual Features
            new Feature
            {
                Name = "Workspaces View",
                Key = FeatureKeys.WorkspacesView,
                Description = "Allows the user to view workspaces.",
                FeatureIndex = FeatureIndex.WorkspacesView,
            },
        };

        if (featuresFromDb.Count > 0)
        {
            // Make sure to only add new features
            Features = Features.Where(f => !featuresFromDb.Any(fdb => fdb.FeatureIndex == f.FeatureIndex)).ToArray();
        }

        await dbContext.Features.AddRangeAsync(Features);
        await dbContext.SaveChangesAsync();

        Features = await dbContext.Features.AsNoTracking().ToListAsync();
    }

    public async Task SeedFeatureLimitations()
    {
        var featureLimitationFromDb = await dbContext.FeatureLimitations.AsNoTracking().ToListAsync();

        FeatureLimitations = new FeatureLimitation[]
        {
            new FeatureLimitation
            {
                Name = "Max Personal Snippets",
                Key = FeatureLimitationKey.MaxPersonalSnippets,
                Description = "The maximum number of personal snippets that can be created.",
                Index = FeatureLimitationIndex.MaxPersonalSnippets,
                FeatureId = GetFeatureId(FeatureIndex.PersonalSnippets),
            },
            new FeatureLimitation
            {
                Name = "Max Personal Snippet Prefixes",
                Key = FeatureLimitationKey.MaxPersonalSnippetPrefixes,
                Description = "The maximum number of personal snippet prefixes that can be created.",
                Index = FeatureLimitationIndex.MaxPersonalSnippetPrefixes,
                FeatureId = GetFeatureId(FeatureIndex.PersonalSnippetPrefixes),
            },
            new FeatureLimitation
            {
                Name = "Max Workspace Snippets",
                Key = FeatureLimitationKey.MaxWorkspaceSnippets,
                Description = "The maximum number of workspace snippets that can be created per workspace.",
                Index = FeatureLimitationIndex.MaxWorkspaceSnippets,
                FeatureId = GetFeatureId(FeatureIndex.Workspaces),
            },
            new FeatureLimitation
            {
                Name = "Max Workspace Snippet Prefixes",
                Key = FeatureLimitationKey.MaxWorkspaceSnippetPrefixes,
                Description = "The maximum number of custom workspace snippet prefixes that can be created.",
                Index = FeatureLimitationIndex.MaxWorkspaceSnippetPrefixes,
                FeatureId = GetFeatureId(FeatureIndex.WorkspaceSnippetPrefixes),
            },
            new FeatureLimitation
            {
                Name = "Max Workspace Invites",
                Key = FeatureLimitationKey.MaxWorkspaceInvites,
                Description = "The maximum number of workspace invites that can be sent per workspace.",
                Index = FeatureLimitationIndex.MaxWorkspaceInvites,
                FeatureId = GetFeatureId(FeatureIndex.Workspaces),
            },
            new FeatureLimitation
            {
                Name = "Max Workspaces",
                Key = FeatureLimitationKey.MaxWorkspaces,
                Description = "The maximum number of workspaces that can be created.",
                Index = FeatureLimitationIndex.MaxWorkspaces,
                FeatureId = GetFeatureId(FeatureIndex.Workspaces),
            },

            // AI Feature Limitations
            new FeatureLimitation
            {
                Name = "Max AI Lenses",
                Key = FeatureLimitationKey.MaxAILenses,
                Description = "The maximum number of AI lenses that can be created.",
                Index = FeatureLimitationIndex.MaxAILenses,
                FeatureId = GetFeatureId(FeatureIndex.AILens),
            }
        };

        if (featureLimitationFromDb.Count > 0)
        {
            // Make sure to only add new feature limitations
            FeatureLimitations = FeatureLimitations.Where(fl => !featureLimitationFromDb.Any(fldb => fldb.Index == fl.Index)).ToArray();
        }

        await dbContext.FeatureLimitations.AddRangeAsync(FeatureLimitations);
        await dbContext.SaveChangesAsync();

        FeatureLimitations = await dbContext.FeatureLimitations.AsNoTracking().ToListAsync();
    }

    private async Task SeedPricingPlanFeatures()
    {
        var pricingPlanFeaturesFromDb = await dbContext.PricingPlanFeatures.AsNoTracking().ToListAsync();

        var freePricingPlanId = GetPricingPlanId(PricingPlanType.Free);
        var basicPricingPlanId = GetPricingPlanId(PricingPlanType.Basic);
        var standardPricingPlanId = GetPricingPlanId(PricingPlanType.Standard);
        var premiumPricingPlanId = GetPricingPlanId(PricingPlanType.Premium);

        PricingPlanFeatures = new PricingPlanFeature[]
        {
            // Free Plan Features
            new PricingPlanFeature
            {
                PricingPlanId = freePricingPlanId,
                FeatureId = GetFeatureId(FeatureIndex.PersonalSnippets),
            },
            new PricingPlanFeature
            {
                PricingPlanId = freePricingPlanId,
                FeatureId = GetFeatureId(FeatureIndex.AIGpt3),
            },
            new PricingPlanFeature
            {
                PricingPlanId = freePricingPlanId,
                FeatureId = GetFeatureId(FeatureIndex.AILens),
            },

            // Basic Plan Features
            new PricingPlanFeature
            {
                PricingPlanId = basicPricingPlanId,
                FeatureId = GetFeatureId(FeatureIndex.PersonalSnippets),
            },
            new PricingPlanFeature
            {
                PricingPlanId = basicPricingPlanId,
                FeatureId = GetFeatureId(FeatureIndex.AIGpt3),
            },
            new PricingPlanFeature
            {
                PricingPlanId = basicPricingPlanId,
                FeatureId = GetFeatureId(FeatureIndex.AILens),
            },
            new PricingPlanFeature
            {
                PricingPlanId = basicPricingPlanId,
                FeatureId = GetFeatureId(FeatureIndex.CustomAiApiKeys),
            },
           
            // Standard Plan Features
            new PricingPlanFeature
            {
                PricingPlanId = standardPricingPlanId,
                FeatureId = GetFeatureId(FeatureIndex.PersonalSnippets),
            },
            new PricingPlanFeature
            {
                PricingPlanId = standardPricingPlanId,
                FeatureId = GetFeatureId(FeatureIndex.PersonalSnippetPrefixes),
            },
            new PricingPlanFeature
            {
                PricingPlanId = standardPricingPlanId,
                FeatureId = GetFeatureId(FeatureIndex.Workspaces),
            },
            new PricingPlanFeature
            {
                PricingPlanId = standardPricingPlanId,
                FeatureId = GetFeatureId(FeatureIndex.AutoSnippetsSync),
            },
            new PricingPlanFeature
            {
                PricingPlanId = standardPricingPlanId,
                FeatureId = GetFeatureId(FeatureIndex.AIGpt3),
            },
            new PricingPlanFeature
            {
                PricingPlanId = standardPricingPlanId,
                FeatureId = GetFeatureId(FeatureIndex.AIGpt4),
            },
            new PricingPlanFeature
            {
                PricingPlanId = standardPricingPlanId,
                FeatureId = GetFeatureId(FeatureIndex.AILens),
            },
            new PricingPlanFeature
            {
                PricingPlanId = standardPricingPlanId,
                FeatureId = GetFeatureId(FeatureIndex.CustomAiApiKeys),
            },

            // Premium Plan Features
            new PricingPlanFeature
            {
                PricingPlanId = premiumPricingPlanId,
                FeatureId = GetFeatureId(FeatureIndex.PersonalSnippets),
            },
            new PricingPlanFeature
            {
                PricingPlanId = premiumPricingPlanId,
                FeatureId = GetFeatureId(FeatureIndex.PersonalSnippetPrefixes),
            },
            new PricingPlanFeature
            {
                PricingPlanId = premiumPricingPlanId,
                FeatureId = GetFeatureId(FeatureIndex.Workspaces),
            },
            new PricingPlanFeature
            {
                PricingPlanId = premiumPricingPlanId,
                FeatureId = GetFeatureId(FeatureIndex.AutoSnippetsSync),
            },
            new PricingPlanFeature
            {
                PricingPlanId = premiumPricingPlanId,
                FeatureId = GetFeatureId(FeatureIndex.WorkspaceSnippetPrefixes),
            },
            new PricingPlanFeature
            {
                PricingPlanId = premiumPricingPlanId,
                FeatureId = GetFeatureId(FeatureIndex.WorkspaceScopedSnippetsEnvironment),
            },
            new PricingPlanFeature
            {
                PricingPlanId = premiumPricingPlanId,
                FeatureId = GetFeatureId(FeatureIndex.AIGpt3),
            },
            new PricingPlanFeature
            {
                PricingPlanId = premiumPricingPlanId,
                FeatureId = GetFeatureId(FeatureIndex.AIGpt4),
            },
            new PricingPlanFeature
            {
                PricingPlanId = premiumPricingPlanId,
                FeatureId = GetFeatureId(FeatureIndex.AILens),
            },
            new PricingPlanFeature
            {
                PricingPlanId = premiumPricingPlanId,
                FeatureId = GetFeatureId(FeatureIndex.CustomAiApiKeys),
            },
        };

        if (pricingPlanFeaturesFromDb.Count > 0)
        {
            // Make sure to only add new pricing plan features
            PricingPlanFeatures = PricingPlanFeatures.Where(
                ppf => !pricingPlanFeaturesFromDb.Any(ppfdb => ppfdb.FeatureId == ppf.FeatureId &&
                                                               ppfdb.PricingPlanId == ppf.PricingPlanId)).ToArray();
        }

        await dbContext.PricingPlanFeatures.AddRangeAsync(PricingPlanFeatures);
        await dbContext.SaveChangesAsync();

        PricingPlanFeatures = await dbContext.PricingPlanFeatures.AsNoTracking().ToListAsync();
    }

    private async Task SeedPricingPlanFeatureLimitations()
    {
        var pricingPlanFeatureLimitationsFromDb = await dbContext.PricingPlanFeatureLimitations.AsNoTracking().ToListAsync();

        var freePricingPlanId = GetPricingPlanId(PricingPlanType.Free);
        var basicPricingPlanId = GetPricingPlanId(PricingPlanType.Basic);
        var standardPricingPlanId = GetPricingPlanId(PricingPlanType.Standard);
        var premiumPricingPlanId = GetPricingPlanId(PricingPlanType.Premium);

        PricingPlanFeatureLimitations = new PricingPlanFeatureLimitation[]
        {
            // Free Plan Limitations
            new PricingPlanFeatureLimitation
            {
                PricingPlanFeatureId = GetPricingPlanFeatureId(freePricingPlanId, GetFeatureId(FeatureIndex.PersonalSnippets)),
                FeatureLimitationId = GetFeatureLimitationId(FeatureLimitationIndex.MaxPersonalSnippets),
                Value = "10",
            },
            new PricingPlanFeatureLimitation
            {
                PricingPlanFeatureId = GetPricingPlanFeatureId(freePricingPlanId, GetFeatureId(FeatureIndex.AILens)),
                FeatureLimitationId = GetFeatureLimitationId(FeatureLimitationIndex.MaxAILenses),
                Value = "1",
            },

            // Basic Plan Limitations
            new PricingPlanFeatureLimitation
            {
                PricingPlanFeatureId = GetPricingPlanFeatureId(basicPricingPlanId, GetFeatureId(FeatureIndex.PersonalSnippets)),
                FeatureLimitationId = GetFeatureLimitationId(FeatureLimitationIndex.MaxPersonalSnippets),
                Value = "300",
            },
            new PricingPlanFeatureLimitation
            {
                PricingPlanFeatureId = GetPricingPlanFeatureId(basicPricingPlanId, GetFeatureId(FeatureIndex.AILens)),
                FeatureLimitationId = GetFeatureLimitationId(FeatureLimitationIndex.MaxAILenses),
                Value = "5",
            },
           
            // Standard Plan Limitations
            new PricingPlanFeatureLimitation {
                PricingPlanFeatureId = GetPricingPlanFeatureId(standardPricingPlanId, GetFeatureId(FeatureIndex.PersonalSnippets)),
                FeatureLimitationId = GetFeatureLimitationId(FeatureLimitationIndex.MaxPersonalSnippets),
                Value = "999",
            },
            new PricingPlanFeatureLimitation
            {
                PricingPlanFeatureId = GetPricingPlanFeatureId(standardPricingPlanId, GetFeatureId(FeatureIndex.PersonalSnippetPrefixes)),
                FeatureLimitationId = GetFeatureLimitationId(FeatureLimitationIndex.MaxPersonalSnippetPrefixes),
                Value = "2",
            },
            new PricingPlanFeatureLimitation
            {
                PricingPlanFeatureId = GetPricingPlanFeatureId(standardPricingPlanId, GetFeatureId(FeatureIndex.Workspaces)),
                FeatureLimitationId = GetFeatureLimitationId(FeatureLimitationIndex.MaxWorkspaces),
                Value = "3",
            },
            new PricingPlanFeatureLimitation
            {
                PricingPlanFeatureId = GetPricingPlanFeatureId(standardPricingPlanId, GetFeatureId(FeatureIndex.Workspaces)),
                FeatureLimitationId = GetFeatureLimitationId(FeatureLimitationIndex.MaxWorkspaceSnippets),
                Value = "500",
            },
            new PricingPlanFeatureLimitation
            {
                PricingPlanFeatureId = GetPricingPlanFeatureId(standardPricingPlanId, GetFeatureId(FeatureIndex.Workspaces)),
                FeatureLimitationId = GetFeatureLimitationId(FeatureLimitationIndex.MaxWorkspaceInvites),
                Value = "5",
            },
            new PricingPlanFeatureLimitation
            {
                PricingPlanFeatureId = GetPricingPlanFeatureId(standardPricingPlanId, GetFeatureId(FeatureIndex.AILens)),
                FeatureLimitationId = GetFeatureLimitationId(FeatureLimitationIndex.MaxAILenses),
                Value = "20",
            },

            // Premium Plan Limitations
            new PricingPlanFeatureLimitation {
                PricingPlanFeatureId = GetPricingPlanFeatureId(premiumPricingPlanId, GetFeatureId(FeatureIndex.PersonalSnippets)),
                FeatureLimitationId = GetFeatureLimitationId(FeatureLimitationIndex.MaxPersonalSnippets),
                Value = int.MaxValue.ToString(),
            },
            new PricingPlanFeatureLimitation
            {
                PricingPlanFeatureId = GetPricingPlanFeatureId(premiumPricingPlanId, GetFeatureId(FeatureIndex.PersonalSnippetPrefixes)),
                FeatureLimitationId = GetFeatureLimitationId(FeatureLimitationIndex.MaxPersonalSnippetPrefixes),
                Value = "50",
            },
            new PricingPlanFeatureLimitation
            {
                PricingPlanFeatureId = GetPricingPlanFeatureId(premiumPricingPlanId, GetFeatureId(FeatureIndex.Workspaces)),
                FeatureLimitationId = GetFeatureLimitationId(FeatureLimitationIndex.MaxWorkspaces),
                Value = "10",
            },
            new PricingPlanFeatureLimitation
            {
                PricingPlanFeatureId = GetPricingPlanFeatureId(premiumPricingPlanId, GetFeatureId(FeatureIndex.Workspaces)),
                FeatureLimitationId = GetFeatureLimitationId(FeatureLimitationIndex.MaxWorkspaceSnippets),
                Value = "1500",
            },
            new PricingPlanFeatureLimitation
            {
                PricingPlanFeatureId = GetPricingPlanFeatureId(premiumPricingPlanId, GetFeatureId(FeatureIndex.Workspaces)),
                FeatureLimitationId = GetFeatureLimitationId(FeatureLimitationIndex.MaxWorkspaceInvites),
                Value = "200",
            },
            new PricingPlanFeatureLimitation
            {
                PricingPlanFeatureId = GetPricingPlanFeatureId(premiumPricingPlanId, GetFeatureId(FeatureIndex.WorkspaceSnippetPrefixes)),
                FeatureLimitationId = GetFeatureLimitationId(FeatureLimitationIndex.MaxWorkspaceSnippetPrefixes),
                Value = "10",
            },
        };

        if (pricingPlanFeatureLimitationsFromDb.Count > 0)
        {
            // Make sure to only add new pricing plan feature limitations
            PricingPlanFeatureLimitations = PricingPlanFeatureLimitations.Where(
                    ppfl => !pricingPlanFeatureLimitationsFromDb.Any(ppflDb => ppflDb.PricingPlanFeatureId == ppfl.PricingPlanFeatureId &&
                                                                               ppflDb.FeatureLimitationId == ppfl.FeatureLimitationId)).ToArray();
        }

        await dbContext.PricingPlanFeatureLimitations.AddRangeAsync(PricingPlanFeatureLimitations);
        await dbContext.SaveChangesAsync();

        PricingPlanFeatureLimitations = await dbContext.PricingPlanFeatureLimitations.AsNoTracking().ToListAsync();
    }

    #region Helper functions
    private Guid GetFeatureId(FeatureIndex featureIndex)
    {
        return Features.First(f => f.FeatureIndex == featureIndex).Id;
    }

    private Guid GetFeatureLimitationId(FeatureLimitationIndex limitationIndex)
    {
        return FeatureLimitations.First(l => l.Index == limitationIndex).Id;
    }

    private Guid GetPricingPlanId(PricingPlanType pricingPlanType)
    {
        return PricingPlans.First(p => p.Type == pricingPlanType).Id;
    }

    private Guid GetPricingPlanFeatureId(Guid pricingPlanId, Guid featureId)
    {
        try
        {
            return PricingPlanFeatures.First(ppf => ppf.PricingPlanId == pricingPlanId && ppf.FeatureId == featureId).Id;
        }
        catch
        {
            return Guid.Empty;
        }
    }
    #endregion
}

