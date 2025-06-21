using Mailjet.Client;
using Mailjet.Client.TransactionalEmails;
using Mailjet.Client.TransactionalEmails.Response;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZODs.Api.Common.Configuration;
using ZODs.Api.Common.Constants;
using ZODs.Api.Common.Helpers;
using ZODs.Api.Common.Interfaces;
using System.Text.Json;

namespace ZODs.Api.Common.Services
{
    public sealed class MailjetEmailService : IEmailService
    {
        private readonly MailjetClient client;
        private readonly MailJetOptions config;
        private readonly EmailOptions emailOptions;
        private readonly ILogger<MailjetEmailService> logger;

        public MailjetEmailService(
            IOptions<MailJetOptions> options,
            IOptions<EmailOptions> mailOpt,
            ILogger<MailjetEmailService> logger)
        {
            this.config = options.Value;
            this.emailOptions = mailOpt.Value;
            this.client = new MailjetClient(config.ApiKey, config.ApiSecret);
            this.logger = logger;
        }

        public Task SendEmailAsync(
            string subject,
            string body,
            string to,
            CancellationToken cancellationToken)
        {
            var builder = GetBuilder(subject)
                .WithHtmlPart(body)
                .WithTo(new SendContact(to));

            var request = builder.Build();

            return client.SendTransactionalEmailAsync(request);
        }

        public async Task SendWelcomeEmailAsync(
            string to,
            string username,
            string name)
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

                var builder = GetBuilder("Welcome to ZODs!")
                    .WithHtmlPart(htmlContent)
                    .WithTo(new SendContact(to));

                var request = builder.Build();

                var response = await client.SendTransactionalEmailAsync(request);

                HandleMailjetResponse(response);

                logger.LogInformation("Welcome email sent to {email}.", to);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending welcome email to {email}.", to);
            }
        }

        public async Task SendEmailConfirmationEmailAsync(
            string to,
            string username,
            string name,
            string emailVerificationUrl)
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

                var builder = GetBuilder("Verify your e-mail address")
                    .WithHtmlPart(htmlContent)
                    .WithTo(new SendContact(to));

                var request = builder.Build();

                var response = await client.SendTransactionalEmailAsync(request);

                HandleMailjetResponse(response);

                logger.LogInformation("Email verification email sent to {email}.", to);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending email verification email to {email}.", to);
            }
        }

        public async Task SendWorkspaceMemberInvitationEmailAsync(
            string to,
            string inviteUrl,
            string workspaceName,
            CancellationToken cancellationToken)
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

                var builder = GetBuilder("You have been invited to a workspace")
                    .WithHtmlPart(htmlContent)
                    .WithTo(new SendContact(to));

                var request = builder.Build();

                var response = await client.SendTransactionalEmailAsync(request);

                HandleMailjetResponse(response);

                logger.LogInformation("Workspace member invitation email sent to {email}.", to);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending workspace member invitation email to {email}.", to);
            }
        }

        public Task<string> SendTemplateEmailAsync(
            string sender,
            List<string> recipients,
            string templateName,
            object templateDataObject,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(string.Empty);
        }

        private TransactionalEmailBuilder GetBuilder(
            string subject)
        {
            var builder = new TransactionalEmailBuilder()
                .WithFrom(new SendContact(emailOptions.Mail, "ZODs"))
                .WithSubject(subject);

            return builder;
        }

        private static void HandleMailjetResponse(TransactionalEmailResponse response)
        {
            if (response.Messages == null || response.Messages.Length == 0)
            {
                throw new Exception("Mailjet response is empty.");
            }

            foreach (var message in response.Messages)
            {
                if (message.Status != "success")
                {
                    throw new Exception($"Mailjet response status is not success. Status: {message.Status}. Errors: {JsonSerializer.Serialize(message.Errors)}");
                }
            }
        }

        public Task SendWelcomeWithEmailVerificationMailAsync(string to, string username, string name, string emailVerificationUrl)
        {
            throw new NotImplementedException();
        }
    }
}