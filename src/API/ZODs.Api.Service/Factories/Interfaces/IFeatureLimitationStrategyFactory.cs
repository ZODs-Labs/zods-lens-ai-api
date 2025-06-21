using ZODs.Api.Repository.Entities.Enums;
using ZODs.Api.Service.Strategies.FeatureLimitationSync.Interfaces;

namespace ZODs.Api.Service.Factories.Interfaces
{
    public interface IFeatureLimitationStrategyFactory
    {
        IFeatureLimitationBaseStrategy Create(FeatureLimitationIndex limitationIndex);
    }
}