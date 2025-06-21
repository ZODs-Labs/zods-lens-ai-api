namespace ZODs.Api.Service.Interfaces
{
    public interface IFeatureLimitationContext
    {
        Guid UserId { get; }

        Guid? WorkspaceId { get; }
    }
}