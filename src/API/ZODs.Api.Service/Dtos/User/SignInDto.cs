using System.ComponentModel.DataAnnotations;

namespace ZODs.Api.Service.Dtos.User;

public sealed class SignInDto
{
    [Required]
    public string EmailAddress { get; set; } = default!;

    [Required]
    public string Password { get; set; } = default!;
}
