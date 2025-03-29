using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace CarBook.Areas.Identity.Pages.Account
{
    public class EmailSender : IEmailSender
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _fromEmail;
        private readonly string _emailPassword;

        public EmailSender(IConfiguration configuration)
        {
            _smtpServer = configuration["Smtp:Server"];
            _smtpPort = int.Parse(configuration["Smtp:Port"]);
            _fromEmail = configuration["Smtp:FromEmail"];
            _emailPassword = configuration["Smtp:Password"];
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            using (var client = new SmtpClient(_smtpServer, _smtpPort))
            {
                client.Credentials = new NetworkCredential(_fromEmail, _emailPassword);
                client.EnableSsl = true;

                var mailMessage = new MailMessage(_fromEmail, email, subject, message) { IsBodyHtml = true };
                await client.SendMailAsync(mailMessage);
            }
        }
    }
}
