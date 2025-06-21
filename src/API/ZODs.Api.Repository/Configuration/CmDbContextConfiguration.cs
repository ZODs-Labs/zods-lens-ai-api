using ZODs.Api.Repository.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ZODs.Api.Repository.Entities.Entities;
using ZODs.Api.Repository.Entities.AI;

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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Identity configuration
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Role>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
            modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(x => x.Id).ValueGeneratedOnAdd();

                entity.HasIndex(u => u.Email)
                     .IsUnique();

                entity.HasMany(u => u.UserSnippets)
                    .WithOne(s => s.User)
                    .HasForeignKey(s => s.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.Workspaces)
                    .WithOne(w => w.User)
                    .HasForeignKey(w => w.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(u => u.SnippetTriggerPrefixes)
                    .WithOne(s => s.User)
                    .HasForeignKey(s => s.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.UserFeatures)
                    .WithOne(uf => uf.User)
                    .HasForeignKey(uf => uf.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserSnippetsVersion>()
                .HasIndex(u => u.UserId)
                .IsUnique();

            modelBuilder.Entity<Workspace>(entity =>
            {
                entity.HasMany(w => w.Members)
                    .WithOne(m => m.Workspace)
                    .HasForeignKey(m => m.WorkspaceId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(w => w.Invites)
                      .WithOne(i => i.Workspace)
                      .HasForeignKey(i => i.WorkspaceId);

                entity.HasMany(w => w.Snippets)
                      .WithOne(s => s.Workspace)
                      .HasForeignKey(s => s.WorkspaceId)
                      .OnDelete(DeleteBehavior.NoAction);
                entity.HasMany(w => w.TriggerPrefixes)
                      .WithOne(s => s.Workspace)
                      .HasForeignKey(s => s.WorkspaceId)
                      .OnDelete(DeleteBehavior.NoAction)
                      .IsRequired(false);
            });

            modelBuilder.Entity<WorkspaceSnippetsVersion>()
                .HasIndex(u => u.WorkspaceId)
                .IsUnique();

            modelBuilder.Entity<WorkspaceMember>(entity =>
            {
                entity.HasMany(m => m.Roles)
                    .WithOne(r => r.WorkspaceMember)
                    .HasForeignKey(r => r.WorkspaceMemberId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(m => new { m.UserId, m.WorkspaceId })
                .IsUnique();
            });

            modelBuilder.Entity<WorkspaceMemberRole>(entity =>
            {
                entity.HasKey(r => new { r.WorkspaceMemberId, r.WorkspaceRoleId });
                entity.HasOne(r => r.WorkspaceMember)
                    .WithMany(w => w.Roles)
                    .HasForeignKey(r => r.WorkspaceMemberId);
                entity.HasOne(r => r.Role)
                    .WithMany(w => w.Members)
                    .HasForeignKey(r => r.WorkspaceRoleId);
            });

            modelBuilder.Entity<WorkspaceSnippet>()
                .HasKey(ws => new { ws.WorkspaceId, ws.SnippetId });

            modelBuilder.Entity<SnippetTriggerPrefix>(entity =>
            {
                entity.HasMany(x => x.Snippets)
                    .WithOne(x => x.TriggerPrefix)
                    .HasForeignKey(x => x.TriggerPrefixId)
                    .IsRequired(false);
            });

            // PricingPlan
            modelBuilder.Entity<PricingPlan>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasMany(e => e.PlanFeatures)
                    .WithOne(ppf => ppf.PricingPlan)
                    .HasForeignKey(ppf => ppf.PricingPlanId);
                entity.HasMany(pp => pp.PricingPlanVariants)
                    .WithOne(ppv => ppv.PricingPlan)
                    .HasForeignKey(ppv => ppv.PricingPlanId);
            });

            // PricingPlanVariant
            modelBuilder.Entity<PricingPlanVariant>(entity =>
            {
                entity.HasIndex(e => new { e.PricingPlanId, e.VariantType }).IsUnique();
                entity.HasMany(e => e.UserPricingPlans)
                    .WithOne(up => up.PricingPlanVariant)
                    .HasForeignKey(up => up.PricingPlanVariantId);
            });

            // Feature
            modelBuilder.Entity<Feature>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasMany(e => e.PlanFeatures)
                    .WithOne(ppf => ppf.Feature)
                    .HasForeignKey(ppf => ppf.FeatureId);
                entity.HasMany(e => e.Limitations)
                    .WithOne(l => l.Feature)
                    .HasForeignKey(l => l.FeatureId);
                entity.HasMany(e => e.UserFeatures)
                    .WithOne(uf => uf.Feature)
                    .HasForeignKey(uf => uf.FeatureId);
            });

            // UserPricingPlan
            modelBuilder.Entity<UserPricingPlan>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.PricingPlanVariantId }).IsUnique();
            });

            // PaymentTransaction
            modelBuilder.Entity<PaymentTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TransactionId).IsUnique();
                entity.HasOne(e => e.User)
                    .WithMany(u => u.PaymentTransactions)
                    .HasForeignKey(e => e.UserId);
            });

            // PricingPlanFeature
            modelBuilder.Entity<PricingPlanFeature>(entity =>
            {
                entity.HasIndex(e => new { e.PricingPlanId, e.FeatureId })
                      .IsUnique();

                entity.HasOne(ppf => ppf.PricingPlan)
                    .WithMany(pp => pp.PlanFeatures)
                    .HasForeignKey(ppf => ppf.PricingPlanId);
                entity.HasOne(ppf => ppf.Feature)
                    .WithMany(f => f.PlanFeatures)
                    .HasForeignKey(ppf => ppf.FeatureId);
                entity.HasMany(ppf => ppf.Limitations)
                    .WithOne(x => x.PricingPlanFeature)
                    .HasForeignKey(x => x.PricingPlanFeatureId);
            });

            modelBuilder.Entity<PricingPlanFeatureRole>()
                .HasKey(pfr => new { pfr.PlanFeatureId, pfr.RoleId });

            modelBuilder.Entity<User>()
                .HasMany(u => u.UserPricingPlans)
                .WithOne(up => up.User)
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PricingPlanFeatureLimitation>()
                .HasKey(x => new { x.PricingPlanFeatureId, x.FeatureLimitationId });

            modelBuilder.Entity<UserFeature>()
                .HasKey(x => new { x.UserId, x.FeatureId });

            // AI Lenses
            modelBuilder.Entity<AILens>(entity =>
            {
                entity.HasOne(x => x.User)
                    .WithMany(x => x.AILenses)
                    .HasForeignKey(x => x.UserId)
                    .IsRequired(false);

                entity.HasOne(x => x.Workspace)
                    .WithMany(x => x.AILenses)
                    .HasForeignKey(x => x.WorkspaceId)
                    .IsRequired(false);

                entity.Property(x => x.IsEnabled)
                    .HasDefaultValue(true);
            });

            // User AI Lens Settings
            modelBuilder.Entity<UserAILensSettings>(entity =>
            {
                entity.HasKey(x => new { x.UserId, x.AILensId });

                entity.HasOne(x => x.User)
                    .WithMany(x => x.UserAILensSettings)
                    .HasForeignKey(x => x.UserId);

                entity.Property(x => x.IsEnabled)
                    .HasDefaultValue(true);
            });

            // User AI Credit Balances
            modelBuilder.Entity<UserAICreditBalance>(entity =>
            {
                entity.HasIndex(x => x.UserId)
                      .IsUnique();

                entity.HasOne(x => x.User)
                    .WithOne(x => x.UserAICreditBalance)
                    .HasForeignKey<UserAICreditBalance>(x => x.UserId);
            });
        }
    }
}