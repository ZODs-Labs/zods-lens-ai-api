namespace ZODs.Api.Repository.Dtos
{
    public sealed class SnippetTriggerPrefixDto
    {
        public Guid Id { get; set; }

        public string Prefix { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
    }
}