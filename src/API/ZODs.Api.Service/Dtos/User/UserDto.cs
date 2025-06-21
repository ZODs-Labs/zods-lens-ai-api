using ZODs.Api.Repository.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace ZODs.Api.Service.Dtos.User;

public sealed class UserDto
{
    public Guid? Id { get; set; }

    [Required]
    [MaxLength(200)]
    [MinLength(1)]
    public string FirstName { get; set; } = null!;

    [Required]
    [MinLength(1)]
    [MaxLength(200)]
    public string LastName { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public UserRegistrationType RegistrationType { get; set; }
}