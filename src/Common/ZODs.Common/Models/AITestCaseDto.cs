using ZODs.Common.Enums;

namespace ZODs.Common.Models;

public class AITestCaseDto : IAITestCaseDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public TestCaseType Type { get; set; }

    public string When { get; set; } = string.Empty;

    public string Given { get; set; } = string.Empty;

    public string Then { get; set; } = string.Empty;
}
