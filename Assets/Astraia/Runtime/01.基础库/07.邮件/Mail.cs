// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-09 16:01:50
// # Recently: 2025-01-11 18:01:29
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Astraia
{
    public static partial class Service
    {
        public static class Mail
        {
            public static async void Send(MailData message)
            {
                try
                {
                    if (string.IsNullOrEmpty(message.senderAddress))
                    {
                        return;
                    }

                    if (string.IsNullOrEmpty(message.senderPassword))
                    {
                        return;
                    }

                    await Task.Run(() =>
                    {
                        var mailMessage = new MailMessage
                        {
                            From = new MailAddress(message.senderAddress, message.senderName),
                            Subject = message.mailName,
                            Body = message.mailBody,
                            IsBodyHtml = false,
                        };
                        mailMessage.To.Add(message.targetAddress);

                        var smtpClient = new SmtpClient(message.smtpServer, message.smtpPort)
                        {
                            UseDefaultCredentials = false,
                            Credentials = new NetworkCredential(message.senderAddress, message.senderPassword),
                            EnableSsl = true,
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            Timeout = 20000
                        };

                        smtpClient.Send(mailMessage);
                    });
                }
                catch (SmtpException e)
                {
                    throw new SmtpException(e.ToString());
                }
            }
        }
    }
}