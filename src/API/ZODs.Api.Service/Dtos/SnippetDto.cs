namespace ZODs.Api.Service.Dtos;

public sealed class SnippetDto
{
    public Guid? Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string CodeSnippet { get; set; } = null!;

    public string BaseTrigger { get; set; } = null!;

    public string Language { get; set; } = null!; 

    public bool IsWorkspaceOwned { get; set; }

    public Guid? WorkspaceId { get; set; }

    public Guid? TriggerPrefixId { get; set; }

    public DateTime CreatedAt { get; set; }
}