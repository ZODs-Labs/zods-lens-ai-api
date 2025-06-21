using ZODs.Api.Repository.Entities;
using ZODs.Api.Repository.Entities.Identity;
using ZODs.Api.Repository.Entities.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ZODs.Api.Repository.Entities.AI;
using ZODs.Api.Repository.Entities.Entities;

namespace ZODs.Api.Repository
{
    public partial class ZodsContext : IdentityDbContext<
        User,
        Role,
        Guid,
        IdentityUserClaim<Guid>,
        IdentityUserRole<Guid>,
        IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>,
        IdentityUserToken<Guid>>
    {
        public ZodsContext(DbContextOptions<ZodsContext> options)
            : base(options)
        {
        }

        // Identity
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        // Domain tables
        public DbSet<Snippet> Snippets { get; set; } = null!;
        public DbSet<UserSnippetsVersion> UserSnippetsVersions { get; set; } = null!;
        public DbSet<Workspace> Workspaces { get; set; } = null!;
        public DbSet<WorkspaceRole> WorkspaceRoles { get; set; } = null!;
        public DbSet<WorkspaceMember> WorkspaceMembers { get; set; } = null!;
        public DbSet<WorkspaceMemberRole> WorkspaceMemberRoles { get; set; } = null!;
        public DbSet<WorkspaceSnippet> WorkspaceSnippets { get; set; } = null!;
        public DbSet<WorkspaceMemberInvite> WorkspaceMemberInvites { get; set; } = null!;
        public DbSet<SnippetTriggerPrefix> SnippetTriggerPrefixes { get; set; }
        public DbSet<WorkspaceSnippetsVersion> WorkspaceSnippetsVersions { get; set; } = null!;
        public DbSet<UserAILensSettings> UserAILensSettings { get; set; } = null!;

        // Pricing plans
        public DbSet<PricingPlan> PricingPlans { get; set; } = null!;
        public DbSet<PricingPlanVariant> PricingPlanVariants { get; set; } = null!;
        public DbSet<PricingPlanFeature> PricingPlanFeatures { get; set; } = null!;
        public DbSet<PricingPlanFeatureRole> PricingPlanFeatureRoles { get; set; } = null!;
        public DbSet<UserPricingPlan> UserPricingPlans { get; set; } = null!;
        public DbSet<UserFeature> UserFeatures { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; } = null!;
        public DbSet<Feature> Features { get; set; } = null!;
        public DbSet<FeatureLimitation> FeatureLimitations { get; set; }
        public DbSet<PricingPlanFeatureLimitation> PricingPlanFeatureLimitations { get; set; }

        // AI
        public DbSet<UserAICreditBalance> UserAICreditBalances { get; set; } = null!;
        public DbSet<AILens> AILenses { get; set; } = null!;

        public DbSet<Subscription> Subscriptions { get; set; } = null!;

        public DbSet<RequestLog> RequestLogs { get; set; }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            OnBeforeSaveChanges();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void OnBeforeSaveChanges()
        {
            ChangeTracker.DetectChanges();

            foreach (var entry in ChangeTracker.Entries().Where(e => e.State is EntityState.Added or EntityState.Modified))
            {
                if (entry.Entity is IAuditableEntity auditableEntity)
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditableEntity.CreatedAt = DateTime.UtcNow;
                            break;
                        case EntityState.Modified:
                            auditableEntity.ModifiedAt = DateTime.UtcNow;
                            break;
                    }
                }
            }
        }
    }
}
