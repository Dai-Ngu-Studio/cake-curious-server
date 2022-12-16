using System.Net;
using System.Net.Mail;

namespace CakeCurious_API.Utilities
{
    public static class SmtpMailSender
    {
        private const string SMTP_AUTOMAILER_PASSWORD = "SMTP_AUTOMAILER_PASSWORD";
        private const string SMTP_HOST = "smtp.gmail.com";
        private const int SMTP_PORT = 587;

        private static string SmtpAutomailerPassword = Environment.GetEnvironmentVariable(SMTP_AUTOMAILER_PASSWORD) ?? string.Empty;

        public static async Task SendStaffMail(string recipientAddress)
        {
            try
            {
                var senderMailAddress = new MailAddress(Environment.GetEnvironmentVariable(EnvironmentHelper.SmtpSenderMailAddress) ?? string.Empty, "");
                var recipientMailAddress = new MailAddress(recipientAddress);
                var smtpClient = new SmtpClient
                {
                    Host = SMTP_HOST,
                    Port = SMTP_PORT,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(senderMailAddress.Address, SmtpAutomailerPassword),
                };

                using (var message = new MailMessage(senderMailAddress, recipientMailAddress)
                {
                    Subject = Environment.GetEnvironmentVariable(EnvironmentHelper.NewStaffMailSubject) ?? "CakeCurious",
                    Body = Environment.GetEnvironmentVariable(EnvironmentHelper.NewStaffMailBody) ?? "CakeCurious",
                    IsBodyHtml = true,
                })
                {
                    await smtpClient.SendMailAsync(message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message}\n{e.InnerException}\n{e.StackTrace}");
            }
        }
    }
}
