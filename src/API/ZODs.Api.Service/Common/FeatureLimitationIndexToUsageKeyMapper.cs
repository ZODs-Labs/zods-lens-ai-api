using ZODs.Api.Common.Constants;
using ZODs.Api.Repository.Entities.Enums;

namespace ZODs.Api.Service.Common
{
    public sealed class FeatureLimitationIndexToUsageKeyMapper
    {
        private static readonly Dictionary<FeatureLimitationIndex, string> mapper = new()
        {
            // Personal Snippets
            { FeatureLimitationIndex.MaxPersonalSnippets, FeatureLimitationUsage.MaxPersonalSnippets },

            // Personal Snippet Prefixes
            { FeatureLimitationIndex.MaxPersonalSnippetPrefixes, FeatureLimitationUsage.MaxPersonalSnippetPrefixes },

            // Workspaces
            { FeatureLimitationIndex.MaxWorkspaces, FeatureLimitationUsage.MaxWorkspaces },
            { FeatureLimitationIndex.MaxWorkspaceSnippets, FeatureLimitationUsage.MaxWorkspaceSnippets },
            { FeatureLimitationIndex.MaxWorkspaceInvites, FeatureLimitationUsage.MaxWorkspaceInvites },

            // Workspace Snippet Prefixes
            { FeatureLimitationIndex.MaxWorkspaceSnippetPrefixes, FeatureLimitationUsage.MaxWorkspaceSnippetPrefixes },

            // AI Lens
            { FeatureLimitationIndex.MaxAILenses, FeatureLimitationUsage.MaxAILenses },
        };

        public static string GetUsageKey(FeatureLimitationIndex limitationIndex)
        {
            return mapper[limitationIndex];
        }
    }
}