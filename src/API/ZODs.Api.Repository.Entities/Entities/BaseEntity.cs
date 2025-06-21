
using ZODs.Api.Repository.Entities.Interfaces;

namespace ZODs.Api.Repository.Entities
{
    public abstract class BaseEntity : IBaseEntity, IAuditableEntity
    {
        public virtual Guid Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ModifiedAt { get; set; }

        public string? CreatedBy { get; set; } = null!;

        public string? ModifiedBy { get; set; } = null!;
    }
}
