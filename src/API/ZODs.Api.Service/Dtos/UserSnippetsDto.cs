using ZODs.Api.Repository.Dtos;

namespace ZODs.Api.Service.Dtos;

public class UserSnippetsForIntegrationDto
{
    public ICollection<SnippetForIntegrationDto> Snippets { get; set; } = new List<SnippetForIntegrationDto>();

    public long SnippetsVersion { get; set; } = 0;
}
