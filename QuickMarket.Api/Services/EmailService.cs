using MailKit.Security;
using MimeKit;

namespace QuickMarket.Api.Services
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string htmlBody);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _cfg;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration cfg, ILogger<EmailService> logger)
        {
            _cfg = cfg;
            _logger = logger;
        }

        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            var fromName = _cfg["Email:FromName"]!;
            var from = _cfg["Email:From"]!;
            var host = _cfg["Email:SmtpHost"]!;
            var portStr = _cfg["Email:SmtpPort"]!;
            var user = _cfg["Email:User"]!;
            var pass = _cfg["Email:Pass"]!;

            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
                throw new InvalidOperationException("Config Email incompleta (From/User/Pass).");

            _logger.LogInformation("SMTP -> Host:{Host} Port:{Port} From:{From} User:{User}", host, portStr, from, user);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, from));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            var port = int.TryParse(portStr, out var p) ? p : 465;
            var secure = port switch
            {
                465 => SecureSocketOptions.SslOnConnect,
                587 => SecureSocketOptions.StartTls,
                _ => SecureSocketOptions.Auto
            };

            if (!string.Equals(from, user, StringComparison.OrdinalIgnoreCase))
                _logger.LogWarning("Para Gmail, es recomendable que Email:From == Email:User para evitar rechazos/spam.");

            using var client = new MailKit.Net.Smtp.SmtpClient();
            try
            {
                await client.ConnectAsync(host, port, secure);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                await client.AuthenticateAsync(user, pass);
                await client.SendAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando correo a {To}", to);
                throw;
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}
