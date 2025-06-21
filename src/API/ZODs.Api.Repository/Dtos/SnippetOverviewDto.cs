namespace ZODs.Api.Repository.Dtos
{
    public sealed class SnippetOverviewDto
    {
        public Guid? Id { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public string Trigger { get; set; } = null!;

        public string Language { get; set; } = null!;

        public DateTime CreatedAt { get; set; } 

        public bool IsEditable { get; set; }
    }
}
