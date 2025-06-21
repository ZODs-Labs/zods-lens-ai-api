using ZODs.Common.Enums;

namespace ZODs.Common;

public interface IAITestCaseDto
{
    Guid Id { get; set; }

    string Title { get; set; }

    TestCaseType Type { get; set; }

    string When { get; set; }

    string Given { get; set; }

    string Then { get; set; }
}
