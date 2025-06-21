using AutoMapper;
using ZODs.Api.Repository.Common;
using ZODs.Api.Repository.Dtos;
using ZODs.Api.Repository.Entities;
using ZODs.Api.Service.Dtos;
using ZODs.Api.Service.InputDtos.Snippet;

namespace ZODs.Api.Service.Mappers.Profiles
{
    public class SnippetMapperProfile : Profile
    {
        public SnippetMapperProfile()
        {
            this.CreateMap<Snippet, SnippetDto>();
            this.CreateMap<SnippetDto, Snippet>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            this.CreateMap<UpsertSnippetInputDto, Snippet>();

            this.CreateMap<PagedEntities<Snippet>, PagedResponse<SnippetDto>>();
            this.CreateMap<PagedEntities<SnippetOverviewDto>, PagedResponse<SnippetOverviewDto>>();
        }
    }
}
