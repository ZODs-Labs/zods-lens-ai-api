using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using ZODs.Api.Common.Configuration;
using ZODs.Api.Common.Constants;
using ZODs.Api.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace ZODs.Api.Common.Services
{
    public sealed class AWSSESService : IEmailService
    {
        private readonly EmailOptions options;
        private readonly IAmazonSimpleEmailService amazonSimpleEmailService;
        private readonly ILogger<AWSSESService> logger;

        public AWSSESService(
            IOptions<EmailOptions> options,
            IAmazonSimpleEmailService amazonSimpleEmailService,
            ILogger<AWSSESService> logger)
        {
            this.options = options.Value;
            if (string.IsNullOrWhiteSpace(this.options?.Mail))
            {
                throw new ArgumentNullException(nameof(options));
            }

            this.amazonSimpleEmailService = amazonSimpleEmailService;
            this.logger = logger;
        }

        public async Task SendEmailAsync(
            string subject,
            string body,
            string to,
            CancellationToken cancellationToken)
        {
            var mailContent = new Content(subject);
            var mailBody = new Body(mailContent);
            var message = new Message(mailContent, mailBody);
            var destination = new Destination(new List<string> { to });

            var request = new SendEmailRequest(this.options.Mail, destination, message);
            await this.amazonSimpleEmailService.SendEmailAsync(request, cancellationToken);
        }

        public async Task SendWelcomeEmailAsync(
            string to,
            string username,
            string name,
            CancellationToken cancellationToken)
        {
            logger.LogInformation("Sending welcome email to {email}.", to);

            try
            {
                await this.SendTemplateEmailAsync(
                    options.Mail,
                    new List<string> { to },
                    EmailTemplateNames.WelcomeTemplate,
                    new
                    {
                        email = username,
                        name,
                        action_url = options.LoginUrl,
                        login_url = options.LoginUrl,
                        support_email = options.SupportEmail,
                        help_url = options.WebHelpPageUrl,
                        logo_url = options.MailLogoUrl,
                    },
                    cancellationToken);

                logger.LogInformation("Welcome email sent to {email}.", to);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending welcome email to {email}.", to);
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
                var paramters = new
                {
                    workspace_name = workspaceName,
                    invite_url = inviteUrl,
                    logo_url = options.MailLogoUrl,
                    help_url = options.WebHelpPageUrl,
                };

                await this.SendTemplateEmailAsync(
                    options.Mail,
                    new List<string> { to },
                    EmailTemplateNames.WorkspaceInvitationTemplate,
                    paramters,
                    cancellationToken);

                logger.LogInformation("Workspace member invitation email sent to {email}.", to);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending workspace member invitation email to {email}.", to);
            }
        }

        /// <summary>
        /// Send an email using a template.
        /// </summary>
        /// <param name="sender">Address of the sender.</param>
        /// <param name="recipients">Addresses of the recipients.</param>
        /// <param name="templateName">Name of the email template.</param>
        /// <param name="templateDataObject">Data for the email template.</param>
        /// <returns>The messageId of the email.</returns>
        public async Task<string> SendTemplateEmailAsync(
            string sender,
            List<string> recipients,
            string templateName,
            object templateDataObject,
            CancellationToken cancellationToken)
        {
            // Template data should be serialized JSON from either a class or a dynamic object.
            var templateData = JsonSerializer.Serialize(templateDataObject);
            var request = new SendTemplatedEmailRequest
            {
                Source = sender,
                Destination = new Destination
                {
                    ToAddresses = recipients
                },
                Template = templateName,
                TemplateData = templateData
            };

            var response = await this.amazonSimpleEmailService.SendTemplatedEmailAsync(request, cancellationToken);
            var messageId = response.MessageId;

            return messageId;
        }

        public Task SendWelcomeEmailAsync(string to, string username, string name)
        {
            throw new NotImplementedException();
        }

        public Task SendEmailConfirmationEmailAsync(string to, string username, string name, string emailVerificationUrl)
        {
            throw new NotImplementedException();
        }

        public Task SendWelcomeWithEmailVerificationMailAsync(string to, string username, string name, string emailVerificationUrl)
        {
            throw new NotImplementedException();
        }
    }
}
