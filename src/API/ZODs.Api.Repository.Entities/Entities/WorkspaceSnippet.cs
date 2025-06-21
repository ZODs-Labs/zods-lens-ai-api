namespace ZODs.Api.Repository.Entities
{
    public sealed class WorkspaceSnippet
    {
        public Guid WorkspaceId { get; set; }

        public Workspace Workspace { get; set; } = null!;

        public Guid SnippetId { get; set; }

        public Snippet Snippet { get; set; } = null!;
    }
}