using System.ComponentModel.DataAnnotations;

namespace ZODs.Api.Models.Input.Auth;

public class GenerateTokenInputDto
{
    [Required]
    public int Purpose { get; set; }
}
