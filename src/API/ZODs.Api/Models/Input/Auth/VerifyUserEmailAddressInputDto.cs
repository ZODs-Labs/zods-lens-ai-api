using ZODs.Common.Attributes;
using System.ComponentModel.DataAnnotations;

namespace ZODs.Api.Models.Input.Auth;

public sealed  class VerifyUserEmailAddressInputDto
{
    [Required]
    [NotEmptyGuid]
    public Guid UserId { get; set; }

    [Required]
    public string Code { get; set; }
}
