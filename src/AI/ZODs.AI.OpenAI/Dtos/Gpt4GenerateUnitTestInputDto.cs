using ZODs.AI.Common.InputDtos.Interfaces;
using ZODs.Common.Attributes;
using ZODs.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace ZODs.AI.OpenAI.Dtos;

public sealed class Gpt4GenerateUnitTestInputDto : IGenerateUnitTestInputDto
{
    public Guid Id { get; set; }

    [MaxLength(1000, ErrorMessage = "Title cannot be longer than 1000 characters.")]
    public string Title { get; set; } = string.Empty;

    [ValidEnum]
    public TestCaseType Type { get; set; }

    [MaxLength(50, ErrorMessage = "Test pattern cannot be longer than 50 characters.")]
    public string TestPattern { get; set; } = string.Empty;

    [MaxLength(50, ErrorMessage = "Test framework cannot be longer than 50 characters.")]
    public string TestFramework { get; set; } = string.Empty;

    [MaxLength(60_000, ErrorMessage = "Tests code is too long.")]
    public string? TestsCode { get; set; }

    [Required(ErrorMessage = "When is required.")]
    [MaxLength(1000, ErrorMessage = "When cannot be longer than 1000 characters.")]
    public string When { get; set; } = string.Empty;

    [Required(ErrorMessage = "Given is required.")]
    [MaxLength(1000, ErrorMessage = "Given cannot be longer than 1000 characters.")]
    public string Given { get; set; } = string.Empty;

    [Required(ErrorMessage = "Then is required.")]
    [MaxLength(1000, ErrorMessage = "Then cannot be longer than 1000 characters.")]
    public string Then { get; set; } = string.Empty;

    [Required(ErrorMessage = "File extension is required.")]
    [MaxLength(10, ErrorMessage = "File extension cannot be longer than 10 characters.")]
    public string FileExtension { get; set; } = string.Empty;

    [Required(ErrorMessage = "Code is required.")]
    [MaxLength(50_000, ErrorMessage = "Code cannot be longer than 50,000 characters.")]
    public string Code { get; set; } = string.Empty;
}
