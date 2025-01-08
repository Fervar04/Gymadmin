using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MailKit.Net.Smtp;
using MimeKit;

namespace APIGym.Services
{
    public class EmailService
    {
        private readonly string _smtpServer;
        private readonly int _port;
        private readonly string _senderName;
        private readonly string _senderEmail;
        private readonly string _username;
        private readonly string _password;

        public EmailService(IConfiguration configuration)
        {
            var emailSettings = configuration.GetSection("EmailSettings");

            // Validar y asignar valores desde la configuración
            _smtpServer = emailSettings["SmtpServer"] ?? throw new ArgumentNullException("SmtpServer configuration is missing.");
            _port = int.TryParse(emailSettings["Port"], out var portValue) ? portValue : throw new ArgumentNullException("Port configuration is missing or invalid.");
            _senderName = emailSettings["SenderName"] ?? throw new ArgumentNullException("SenderName configuration is missing.");
            _senderEmail = emailSettings["SenderEmail"] ?? throw new ArgumentNullException("SenderEmail configuration is missing.");
            _username = emailSettings["Username"] ?? throw new ArgumentNullException("Username configuration is missing.");
            _password = emailSettings["Password"] ?? throw new ArgumentNullException("Password configuration is missing.");
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_senderName, _senderEmail));
            email.To.Add(new MailboxAddress("", toEmail));
            email.Subject = subject;
            email.Body = new TextPart("html") { Text = message };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_smtpServer, _port, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_username, _password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}