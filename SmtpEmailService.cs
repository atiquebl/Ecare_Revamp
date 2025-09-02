using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;
namespace ECare_Revamp.Models
{
    public class SmtpSettings
    {
        public string? Host { get; set; }
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public string? From { get; set; }
    }
    public interface IEmailService
    {
        Task SendAsync(string toEmail, string ccEmail, string subject, string body);
    }
    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public SmtpEmailService(IConfiguration configuration)
        {
            _smtpSettings = configuration.GetSection("SmtpSettings").Get<SmtpSettings>()
                            ?? throw new ArgumentNullException(nameof(configuration), "SmtpSettings section is missing or invalid.");
        }

        public async Task SendAsync(string toEmail, string ccEmail, string subject, string body)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_smtpSettings.From))
                {
                    throw new InvalidOperationException("The 'From' address in SMTP settings cannot be null or empty.");
                }

                using var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
                {
                    EnableSsl = _smtpSettings.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpSettings.From,"E-Care"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false // change to true if sending HTML email
                };
                mailMessage.To.Add(toEmail);
                mailMessage.CC.Add(ccEmail); // Clear CC if not needed
                ContentType mimeType = new System.Net.Mime.ContentType("text/html");
                AlternateView alternate = AlternateView.CreateAlternateViewFromString(body, mimeType);
                mailMessage.AlternateViews.Add(alternate);
                await client.SendMailAsync(mailMessage);
            }
            catch (SmtpException smtpEx)
            {
                throw new Exception($"SMTP Error: {smtpEx.Message}", smtpEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Exception Error: {ex.Message}", ex);
            }
        }
    }
}
