using ZODs.Api.Repository.Entities.Entities;

namespace ZODs.Api.Repository.Entities;

public sealed class UserAILensSettings
{
    public bool IsEnabled { get; set; }

    public Guid AILensId { get; set; }
    public AILens AILens { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}