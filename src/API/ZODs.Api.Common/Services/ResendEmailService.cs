using ZODs.Api.Common.Configuration;
using ZODs.Api.Common.Constants;
using ZODs.Api.Common.Helpers;
using ZODs.Api.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resend;

namespace ZODs.Api.Common.Services;

public sealed class ResendEmailService(
    IResend resend,
    ILogger<ResendEmailService> logger,
    IOptions<EmailOptions> emailOptions) : IEmailService
{
    private readonly IResend resend = resend;
    private readonly ILogger<ResendEmailService> logger = logger;
    private readonly EmailOptions emailOptions = emailOptions.Value;

    public async Task SendEmailConfirmationEmailAsync(string to, string username, string name, string emailVerificationUrl)
    {
        logger.LogInformation("Sending email confirmation email to {email}.", to);

        try
        {
            var htmlContent = MailTemplateHelper.GetHtmlMailTemplateAsString(
                EmbeddedResources.EmailConfirmationMailTemplate,
                new Dictionary<string, string>
                {
                        { "email", username },
                        { "name", name },
                        { "action_url", emailVerificationUrl },
                        { "login_url", emailOptions.LoginUrl },
                        { "support_email", emailOptions.SupportEmail },
                        { "logo_url", emailOptions.MailLogoUrl },
                });

            var message = new EmailMessage
            {
                From = emailOptions.Mail,
                Subject = "Verify your e-mail address",
                To = to,
                HtmlBody = htmlContent,
            };

            await resend.EmailSendAsync(message);

            logger.LogInformation("Email verification email sent to {email}.", to);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending email verification email to {email}.", to);
        }
    }

    public async Task SendWelcomeWithEmailVerificationMailAsync(string to, string username, string name, string emailVerificationUrl)
    {
        logger.LogInformation("Sending welcome email to {email}.", to);

        try
        {
            var htmlContent = MailTemplateHelper.GetHtmlMailTemplateAsString(
              EmbeddedResources.WelcomeWithEmailVerificationMailTemplate,
              new Dictionary<string, string>
              {
                      { "email", username },
                      { "name", name },
                      { "action_url", emailVerificationUrl },
                      { "login_url", emailOptions.LoginUrl },
                      { "support_email", emailOptions.SupportEmail },
                      { "help_url", emailOptions.WebHelpPageUrl },
                      { "logo_url", emailOptions.MailLogoUrl },
              });

            var message = new EmailMessage
            {
                From = emailOptions.Mail,
                Subject = "Welcome to ZODs",
                To = to,
                HtmlBody = htmlContent,
            };

            await resend.EmailSendAsync(message);

            logger.LogInformation("Welcome email sent to {email}.", to);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending welcome email to {email}.", to);
        }
    }

    public async Task SendWelcomeEmailAsync(string to, string username, string name)
    {
        logger.LogInformation("Sending welcome email to {email}.", to);

        try
        {
            var htmlContent = MailTemplateHelper.GetHtmlMailTemplateAsString(
              EmbeddedResources.WelcomeTemplate,
              new Dictionary<string, string>
              {
                      { "email", username },
                      { "name", name },
                      { "login_url", emailOptions.LoginUrl },
                      { "support_email", emailOptions.SupportEmail },
                      { "help_url", emailOptions.WebHelpPageUrl },
                      { "logo_url", emailOptions.MailLogoUrl },
              });

            var message = new EmailMessage
            {
                From = emailOptions.Mail,
                Subject = "Welcome to ZODs",
                To = to,
                HtmlBody = htmlContent,
            };

            await resend.EmailSendAsync(message);

            logger.LogInformation("Welcome email sent to {email}.", to);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending welcome email to {email}.", to);
        }
    }

    public async Task SendWorkspaceMemberInvitationEmailAsync(string to, string inviteUrl, string workspaceName, CancellationToken cancellationToken)
    {
        logger.LogInformation("Sending workspace member invitation email to {email}.", to);

        try
        {
            var htmlContent = MailTemplateHelper.GetHtmlMailTemplateAsString(
                 EmbeddedResources.WorkspaceMemberInvitationTemplate,
                 new Dictionary<string, string>
                 {
                      { "workspace_name", workspaceName },
                      { "invite_url", inviteUrl },
                      { "help_url", emailOptions.WebHelpPageUrl },
                      { "logo_url", emailOptions.MailLogoUrl },
                 });

            var message = new EmailMessage
            {
                From = emailOptions.Mail,
                Subject = "Workspace Invitation",
                To = to,
                HtmlBody = htmlContent,
            };

            await resend.EmailSendAsync(message, cancellationToken);

            logger.LogInformation("Workspace member invitation email sent to {email}.", to);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending workspace member invitation email to {email}.", to);
        }
    }

    public Task<string> SendTemplateEmailAsync(string sender, List<string> recipients, string templateName, object templateDataObject, CancellationToken cancellationToken)
    {
        return Task.FromResult(string.Empty);
    }

    public Task SendEmailAsync(string subject, string body, string to, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
