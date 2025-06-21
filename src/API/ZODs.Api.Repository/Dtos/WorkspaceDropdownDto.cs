namespace ZODs.Api.Repository.Dtos;

public sealed class WorkspaceDropdownDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public bool IsReachedMaxSnippetsLimitation { get; set; }
}

