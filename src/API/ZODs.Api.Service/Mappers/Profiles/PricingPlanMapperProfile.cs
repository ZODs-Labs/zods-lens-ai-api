using AutoMapper;
using ZODs.Api.Repository.Entities;
using ZODs.Api.Service.InputDtos.PricingPlan;

namespace ZODs.Api.Service.Mappers.Profiles;

public sealed class PricingPlanMapperProfile : Profile
{
    public PricingPlanMapperProfile()
    {
        this.CreateMap<UpsertUserPricingPlanInputDto, UserPricingPlan>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
    }
}