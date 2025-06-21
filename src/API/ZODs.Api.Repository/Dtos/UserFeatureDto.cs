using ZODs.Api.Repository.Entities.Enums;

namespace ZODs.Api.Repository.Dtos;

public sealed class UserFeatureDto
{
    public FeatureIndex FeatureIndex { get; set; }

    public string Key { get; set; } = null!;
}