using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EmailService
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailConfiguration _emailConfig;

        public EmailSender(EmailConfiguration emailConfig)
        {
            _emailConfig = emailConfig;
        }

        public async Task SendEmail(Message message)
        {
            var emailMessage = CreateEmailMessage(message);
            await SendEmailMessage(emailMessage);
        }

        private async Task SendEmailMessage(MimeMessage emailMessage)
        {
            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, true);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    await client.AuthenticateAsync(_emailConfig.Username, _emailConfig.Password);
                    await client.SendAsync(emailMessage);
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    await client.DisconnectAsync(true);
                    client.Dispose();
                }
            }
        }

        private MimeMessage CreateEmailMessage(Message message)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress(_emailConfig.From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = "Folder Watcher 2.0 : "+message.Subject;
            var messageBody = new BodyBuilder { HtmlBody = @"<table border='1' cellpadding='0' cellspacing='0' width='100 %' style='border - collapse: collapse; font - size: 1.2em; font - weight: bold;'>
              <tr>
                <td bgcolor = '#ffffff' style='padding: 40px 30px 40px 30px; background: linear-gradient(to top left, red, blue); color: white;'> Folder Watcher 2.0 </td>
                     </ tr >
                     <tr>
                         <td style='padding: 20px 15px 20px 15px;'>" +message.Content+ "</td></tr></table>" };

            if (!string.IsNullOrWhiteSpace(message.Attachment))
            {
                var file = new FileInfo(message.Attachment);
                messageBody.Attachments.Add(file.Name, File.ReadAllBytes(message.Attachment));
            }
            emailMessage.Body = messageBody.ToMessageBody();

            return emailMessage;
        }
    }
}
