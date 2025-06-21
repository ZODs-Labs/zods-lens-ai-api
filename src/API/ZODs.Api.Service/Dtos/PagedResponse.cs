namespace ZODs.Api.Service.Dtos;

public class PagedResponse<TDto>
{
    public List<TDto> Entities { get; set; } = new List<TDto>();

    public int TotalCount { get; set; } = 0;
}
