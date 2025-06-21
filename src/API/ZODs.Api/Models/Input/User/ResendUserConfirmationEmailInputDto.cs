using ZODs.Common.Attributes;
using System.ComponentModel.DataAnnotations;

namespace ZODs.Api.Models.Input.User
{
    public class ResendUserConfirmationEmailInputDto
    {
        [Required]
        [NotEmptyGuid]
        public Guid UserId { get; set; }
    }
}
