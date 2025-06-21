using AutoMapper;
using ZODs.Api.Repository.Common;
using ZODs.Api.Repository.Dtos;
using ZODs.Api.Repository.Entities;
using ZODs.Api.Service.Dtos;
using ZODs.Api.Service.InputDtos;

namespace ZODs.Api.Service.Mappers.Profiles
{
    public class SnippetTriggerPrefixMapperProfile : Profile
    {
        public SnippetTriggerPrefixMapperProfile()
        {
            this.CreateMap<SnippetTriggerPrefix, SnippetTriggerPrefixDto>();
            this.CreateMap<SnippetTriggerPrefixInputDto, SnippetTriggerPrefix>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            this.CreateMap<PagedEntities<SnippetTriggerPrefix>, PagedResponse<SnippetTriggerPrefixDto>>();
        }
    }
}
