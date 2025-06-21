namespace ZODs.AI.Common.InputDtos.Interfaces;

public interface IGenerateUnitTestCasesInputDto
{
    string Code { get; set; }

    string FileExtension { get; set; }

    string? TestsCode { get; set; }

    int NumberOfTestCases { get; set; }

    int TotalPositiveTestCases { get; set; }

    int TotalNegativeTestCases { get; set; }

    int TotalEdgeCaseTestCases { get; set; }
}
