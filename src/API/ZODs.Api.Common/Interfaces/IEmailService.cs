namespace ZODs.Api.Common.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string subject, string body, string to, CancellationToken cancellationToken);

        Task SendWelcomeEmailAsync(
            string to,
            string username,
            string name);

        Task SendWorkspaceMemberInvitationEmailAsync(
            string to,
            string inviteUrl,
            string workspaceName,
            CancellationToken cancellationToken);

        Task<string> SendTemplateEmailAsync(
            string sender,
            List<string> recipients,
            string templateName,
            object templateDataObject,
            CancellationToken cancellationToken);
        Task SendEmailConfirmationEmailAsync(string to, string username, string name, string emailVerificationUrl);
        Task SendWelcomeWithEmailVerificationMailAsync(string to, string username, string name, string emailVerificationUrl);
    }
}
