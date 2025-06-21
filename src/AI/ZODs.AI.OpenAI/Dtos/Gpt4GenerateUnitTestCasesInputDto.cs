using ZODs.AI.Common.InputDtos.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace ZODs.AI.OpenAI.Dtos;

public sealed class Gpt4GenerateUnitTestCasesInputDto : IGenerateUnitTestCasesInputDto
{
    [Length(1, 10_000, ErrorMessage = "Code must be between 1 and 10,000 characters")]
    public string Code { get; set; } = string.Empty;

    [MaxLength(10, ErrorMessage = "File extension is too long.")]
    public string FileExtension { get; set; } = string.Empty;

    [MaxLength(60_000, ErrorMessage = "Tests code is too long.")]
    public string? TestsCode { get; set; }

    [Range(1, 10, ErrorMessage = "Number of test cases must be between 1 and 10")]
    public int NumberOfTestCases { get; set; }

    [Range(0, 10, ErrorMessage = "Total positive test cases must be between 0 and 10")]
    public int TotalPositiveTestCases { get; set; }

    [Range(0, 10, ErrorMessage = "Total negative test cases must be between 0 and 10")]
    public int TotalNegativeTestCases { get; set; }

    [Range(0, 10, ErrorMessage = "Total edge case test cases must be between 0 and 10")]
    public int TotalEdgeCaseTestCases { get; set; }
}
