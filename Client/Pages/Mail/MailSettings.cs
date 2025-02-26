using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace ReservationBookingSystem.Services
{
    public interface IMailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlBody, string password);
    }
    public class MailService : IMailService
    {
        private readonly MailSettings? _mailSettings;

        public MailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }
        public string? FromEmail { get; set; }
        public string? Host { get; set; }
        public int Port { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody, string password)
        {
            // Implementacja metody wysyłającej wiadomość e-mail
            Console.WriteLine($"Sending email to {toEmail} with subject '{subject}' Host: {_mailSettings.Host} Port: {_mailSettings.Port}");

            if (password == "---")
            {
                password = _mailSettings.Password;
            }

            MailMessage message = new MailMessage();
            SmtpClient smtp = new SmtpClient();
            // message.From = new MailAddress(_mailSettings.FromEmail);
            message.From = new MailAddress(toEmail);
            message.To.Add(new MailAddress(toEmail));
            message.Subject = subject;
            message.IsBodyHtml = true;
            message.Body = htmlBody;
            smtp.Port = _mailSettings.Port;
            smtp.Host = _mailSettings.Host;
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(_mailSettings.FromEmail, password);
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            await smtp.SendMailAsync(message);

            await Task.CompletedTask; // Dummy async operation
        }
    }
    public class MailSettings
    {
        public string? FromEmail { get; set; }
        public string? Host { get; set; }
        public int Port { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}
