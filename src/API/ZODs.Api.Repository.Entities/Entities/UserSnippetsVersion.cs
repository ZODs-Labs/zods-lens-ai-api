using System.ComponentModel.DataAnnotations;

namespace ZODs.Api.Repository.Entities
{
    public sealed class UserSnippetsVersion : BaseEntity
    {
        [Required]
        public long SnippetVersion { get; set; }

        public Guid UserId { get; set; }

        public User User { get; set; } = null!;
    }
}
