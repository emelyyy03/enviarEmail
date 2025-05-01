using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace EnviarEmail.AppWebMVC.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, List<IFormFile>? attachments = null)
        {
            // Validar configuración
            string? from = _config["EmailSettings:From"];
            string? smtpServer = _config["EmailSettings:SmtpServer"];
            string? portStr = _config["EmailSettings:Port"];
            string? username = _config["EmailSettings:Username"];
            string? password = _config["EmailSettings:Password"];

            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(smtpServer)
                || string.IsNullOrWhiteSpace(portStr) || string.IsNullOrWhiteSpace(username)
                || string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException("Faltan configuraciones de Email en appsettings.json.");
            }

            if (!int.TryParse(portStr, out int port))
            {
                throw new InvalidOperationException("El puerto SMTP no es válido.");
            }

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(from));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };

            // Agregar adjuntos si existen
            if (attachments != null)
            {
                foreach (var file in attachments)
                {
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    ms.Position = 0;
                    builder.Attachments.Add(file.FileName, ms.ToArray());
                }
            }

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(smtpServer, port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(username, password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
