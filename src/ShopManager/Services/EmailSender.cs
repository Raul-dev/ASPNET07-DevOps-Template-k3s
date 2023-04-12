using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using System;
using ShopManager.Interfaces;
using ShopManager.Models;
using Serilog;

namespace ShopManager.Services
{
    public class EmailSender : IEmailSmtpSender
    {
        private readonly IOptions<EmailSettings> _optionsEmailSettings;

        
        public EmailSender(IOptions<EmailSettings> optionsEmailSettings)
        {
            _optionsEmailSettings = optionsEmailSettings;
            
        }

        public bool ValidateSmtp(out string responseData)
        {
            responseData = "";
            return true;
            /*
            bool res = SmtpHelper.ValidateCredentials("nevadrive24@mail.ru", "1QEadzc2", "smtp.mail.ru", 25, false, out responseData);
            if (!res)
                _logger.LogError(responseData);
            else
                _logger.LogInformation(responseData);
            return res;
            */
        }
        public async Task<bool> ValidateSmtpAsync()
        {
            var res = await Task<bool>.Run(() =>
            {
                string responseData;
                return ValidateSmtp( out responseData);
            });
            return res;
        }

        public async Task SendEmail(string email, string subject, string message, string toUsername)
        {
            try { 
            SmtpClient smtp1 = new SmtpClient(_optionsEmailSettings.Value.Server, _optionsEmailSettings.Value.Port);
            smtp1.Credentials = new NetworkCredential(_optionsEmailSettings.Value.User, _optionsEmailSettings.Value.Password);
            MailMessage msg = new MailMessage();
            msg.From = new MailAddress(_optionsEmailSettings.Value.User, "information nevadrive24");
            //string localemail = "nevadrive24@mail.ru";
            msg.To.Add(new MailAddress(email));
            msg.Subject = subject;
            msg.IsBodyHtml = true;
            msg.Body = message + " <b> To " + email + " </b>";
            smtp1.EnableSsl = _optionsEmailSettings.Value.EnableSsl;
            smtp1.DeliveryMethod = SmtpDeliveryMethod.Network;

            await smtp1.SendMailAsync(msg);
            Log.Information("Confirmation message was send to {0}.", email);
                
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Message didnt send to {0}.", email);
                
            }
        }

    }
}
