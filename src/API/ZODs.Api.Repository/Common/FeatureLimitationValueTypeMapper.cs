using ZODs.Api.Repository.Entities.Enums;

namespace ZODs.Api.Repository.Common
{
    public static class FeatureLimitationValueTypeMapper
    {
        public static readonly Dictionary<FeatureLimitationIndex, Type> TypeMap = new()
        {
            // Personal Snippets
            {FeatureLimitationIndex.MaxPersonalSnippets, typeof(int)},

            // Personal Snippet Prefixes
            {FeatureLimitationIndex.MaxPersonalSnippetPrefixes, typeof(int)},

            // Workspaces
            {FeatureLimitationIndex.MaxWorkspaces, typeof(int)},
            {FeatureLimitationIndex.MaxWorkspaceSnippets, typeof(int)},
            {FeatureLimitationIndex.MaxWorkspaceInvites, typeof(int)},

            // Workspace Snippet Prefixes
            {FeatureLimitationIndex.MaxWorkspaceSnippetPrefixes, typeof(int)},

            // AI Lenses
            {FeatureLimitationIndex.MaxAILenses, typeof(int)},
        };

        public static object MapValue(FeatureLimitationIndex limitationIndex, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var type = TypeMap[limitationIndex];

            return type switch
            {
                Type intType when intType == typeof(int) => int.Parse(value),
                Type stringType when stringType == typeof(string) => value,
                Type boolType when boolType == typeof(bool) => bool.Parse(value),
                _ => throw new ArgumentOutOfRangeException(nameof(limitationIndex), type, null)
            };
        }
    }
}