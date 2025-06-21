namespace ZODs.Common;

public class CommonConstants
{
    public const string UtcTimezone = "UTC";

    public static readonly DateTime EpochDateTime = DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Utc);
}