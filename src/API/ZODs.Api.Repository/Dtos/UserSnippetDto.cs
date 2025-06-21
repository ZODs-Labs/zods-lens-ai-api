namespace ZODs.Api.Repository.Dtos
{
    public sealed class UserSnippetDto
    {
        public Guid? Id { get; set; }

        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string CodeSnippet { get; set; } = null!;

        public string Trigger { get; set; } = null!;

        public string Language { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
    }
}
