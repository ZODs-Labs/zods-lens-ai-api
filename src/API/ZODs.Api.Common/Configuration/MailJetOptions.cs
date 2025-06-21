namespace ZODs.Api.Common.Configuration;

public sealed class MailJetOptions
{
    public string ApiKey { get; set; }

    public string ApiSecret { get; set; }

    public string FromEmail { get; set; }
}