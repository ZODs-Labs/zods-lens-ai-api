using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Service.Strategies.FeatureLimitationSync.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using ZODs.Api.Service.Strategies.FeatureLimitationSync;
using ZODs.Api.Service.Factories.Interfaces;

namespace ZODs.Api.Service.Factories
{
    public class FeatureLimitationStrategyFactory : IFeatureLimitationStrategyFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public FeatureLimitationStrategyFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IFeatureLimitationBaseStrategy Create(FeatureLimitationIndex limitationIndex)
        {
            return limitationIndex switch
            {
                // Personal Snippets
                FeatureLimitationIndex.MaxPersonalSnippets => _serviceProvider.GetRequiredService<MaxPersonalSnippetsLimitationStrategy>(),

                // Personal Snippet Prefixes
                FeatureLimitationIndex.MaxPersonalSnippetPrefixes => _serviceProvider.GetRequiredService<MaxPersonalSnippetPrefixesLimitationStrategy>(),

                // Workspaces
                FeatureLimitationIndex.MaxWorkspaces => _serviceProvider.GetRequiredService<MaxWorkspacesLimitationStrategy>(),
                FeatureLimitationIndex.MaxWorkspaceSnippets => _serviceProvider.GetRequiredService<MaxWorkspaceSnippetsLimitationStrategy>(),
                FeatureLimitationIndex.MaxWorkspaceInvites => _serviceProvider.GetRequiredService<MaxWorkspaceInvitesLimitationStrategy>(),

                // Workspace Snippet Prefixes
                FeatureLimitationIndex.MaxWorkspaceSnippetPrefixes => _serviceProvider.GetRequiredService<MaxWorkspaceSnippetPrefixesLimitationStrategy>(),

                // AI Lens
                FeatureLimitationIndex.MaxAILenses => _serviceProvider.GetRequiredService<MaxAILensesLimitationStrategy>(),

                _ => throw new NotImplementedException($"Sync strategy for {limitationIndex} is not implemented.")
            };
        }
    }
}