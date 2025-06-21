namespace ZODs.Api.Common.Constants;

public static class EmbeddedResources
{
    public const string MailTemplatesNamespace = "ZODs.Api.Common.Templates.Email";

    public const string WelcomeTemplate = $"{MailTemplatesNamespace}.WelcomeMailTemplate.html";
    public const string WorkspaceMemberInvitationTemplate = $"{MailTemplatesNamespace}.WorkspaceMemberInvitationMailTemplate.html";
    public const string EmailConfirmationMailTemplate = $"{MailTemplatesNamespace}.EmailConfirmationMailTemplate.html";
    public const string WelcomeWithEmailVerificationMailTemplate = $"{MailTemplatesNamespace}.{EmailTemplateNames.WelcomeWithEmailVerificationMailTemplate}.html";
}