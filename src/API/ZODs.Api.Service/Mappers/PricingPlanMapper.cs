using AutoMapper;
using ZODs.Api.Repository.Entities;
using ZODs.Api.Service.InputDtos.PricingPlan;
using ZODs.Api.Service.Mappers.Profiles;

namespace ZODs.Api.Service.Mappers
{
    public static class PricingPlanMapper
    {

        static PricingPlanMapper()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<PricingPlanMapperProfile>()).CreateMapper();
        }

        public static IMapper Mapper { get; }

        /// <summary>
        /// Maps <see cref="UserPricingPlan"/> to <see cref="UserPricingPlanDto"/>.
        /// </summary>
        /// <param name="dto">Entity to map from.</param></param>
        /// <returns>Mapped <see cref="UserPricingPlanDto"/>.</returns>
        public static UserPricingPlan ToPricingPlan(this UpsertUserPricingPlanInputDto dto)
        {
            return Mapper.Map<UserPricingPlan>(dto);
        }
    }
}