namespace ZODs.Api.Repository.Entities;

public sealed class UserFeature
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public Guid FeatureId { get; set; }
    public Feature? Feature { get; set; }

    public DateTime CreatedAt { get; set; }
}