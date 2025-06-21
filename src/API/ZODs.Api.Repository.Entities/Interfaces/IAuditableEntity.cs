namespace ZODs.Api.Repository.Entities.Interfaces
{
    public interface IAuditableEntity
    {
        public DateTime CreatedAt { get; set; }

        public DateTime? ModifiedAt { get; set; }

        public string? CreatedBy { get; set; }

        public string? ModifiedBy { get; set;}
    }
}
