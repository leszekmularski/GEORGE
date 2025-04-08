﻿using Microsoft.Extensions.Options;
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
            Console.WriteLine($"Serwer -> Sending email to {toEmail} with subject '{subject}' Host: {_mailSettings.Host} Port: {_mailSettings.Port}");

            if (password == "---")
            {
                password = _mailSettings.Password;
            }

            using (var smtp = new SmtpClient())
            {
                smtp.Host = _mailSettings.Host;            // mail.george.pl
                smtp.Port = _mailSettings.Port;            // 465
                smtp.EnableSsl = true;                     // SSL/TLS
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(_mailSettings.FromEmail, password);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                var message = new MailMessage
                {
                    From = new MailAddress(_mailSettings.FromEmail),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                message.To.Add(toEmail);

                try
                {
                    await smtp.SendMailAsync(message);
                    Console.WriteLine("✅ Email sent successfully.");
                }
                catch (SmtpException ex)
                {
                    Console.WriteLine($"❌ SMTP error: {ex.StatusCode} - {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ General error: {ex.Message}");
                }
            }

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
