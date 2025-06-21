using ZODs.Common.Enums;

namespace ZODs.AI.Common.InputDtos.Interfaces;

public interface IGenerateUnitTestInputDto
{
    Guid Id { get; set; }

    string Title { get; set; }

    TestCaseType Type { get; set; }

    string TestPattern { get; set; }

    string TestFramework { get; set; }

    string? TestsCode { get; set; }

    string When { get; set; }

    string Given { get; set; }

    string Then { get; set; }

    string Code { get; set; }

    string FileExtension { get; set; }
}
