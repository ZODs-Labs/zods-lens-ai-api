using ZODs.Api.Repository.Entities.Enums;

namespace ZODs.Api.Service.Common
{
    public static class FeatureLimitationMessages
    {
        private static readonly Dictionary<FeatureLimitationIndex, string> LimitationIndexMessagesMap = new()
        {
            { FeatureLimitationIndex.MaxPersonalSnippets, "You've reached the maximum number of personal snippets allowed for your pricing plan." },
            { FeatureLimitationIndex.MaxPersonalSnippetPrefixes, "You've reached the maximum number of personal snippet prefixes allowed for your pricing plan." },
            { FeatureLimitationIndex.MaxWorkspaceSnippets, "You've reached the maximum number of snippets allowed for this workspace in your pricing plan." },
            { FeatureLimitationIndex.MaxWorkspaceSnippetPrefixes, "You've reached the maximum number of snippet prefixes allowed for this workspace in your pricing plan." },
            { FeatureLimitationIndex.MaxWorkspaceInvites, "You've reached the maximum number of invites allowed for workspaces in your pricing plan." },
            { FeatureLimitationIndex.MaxWorkspaces, "You've reached the maximum number of workspaces allowed for your pricing plan." }
        };

        public static string GetLimitationMessage(FeatureLimitationIndex limitationIndex)
        {
            LimitationIndexMessagesMap.TryGetValue(limitationIndex, out var message);
            return message ?? string.Empty;
        }
    }
}