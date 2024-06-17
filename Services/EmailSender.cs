using MailKit;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.IO.Pipelines;
using MailKit.Net.Smtp;

namespace Ruddy.WEB.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            if (!CheakEmailAddress(email) || !CheakParams(subject, message))
                return;

            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress(_configuration["SMTP:SenderName"], _configuration["SMTP:User"]));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };

            try
            {
                var test = _configuration["SMTP:User"];

                using (var client = new SmtpClient())
                {
                    client.CheckCertificateRevocation = false;
                    await client.ConnectAsync(_configuration["SMTP:Host"], Int32.Parse(_configuration["SMTP:Port"]), true).ConfigureAwait(false);
                    await client.AuthenticateAsync(_configuration["SMTP:User"], _configuration["SMTP:Key"]).ConfigureAwait(false);
                    await client.SendAsync(emailMessage).ConfigureAwait(false);
                    await client.DisconnectAsync(true).ConfigureAwait(false);
                }
            }
            catch
            {
                throw;
            }


        }

        private static bool CheakParams(string subject, string message)
        {
            if (string.IsNullOrWhiteSpace(subject))
                return false;

            if (string.IsNullOrWhiteSpace(message))
                return false;

            return true;
        }

        private static bool CheakEmailAddress(string email)
        {
            if (!string.IsNullOrWhiteSpace(email) && email.Contains("@"))
                return true;
            return false;
        }

    }
}
