using ZODs.Api.Repository.Dtos;

namespace ZODs.Api.Service.Dtos
{
    public sealed class UserSnippetsOverviewDto : PagedResponse<SnippetOverviewDto>
    {
        public long SnippetsVersion { get; set; }

        public UserSnippetsOverviewDto(PagedResponse<SnippetOverviewDto> pagedSnippets)
        {
            this.Entities = pagedSnippets.Entities;
            this.TotalCount = pagedSnippets.TotalCount;
        }
    }
}
