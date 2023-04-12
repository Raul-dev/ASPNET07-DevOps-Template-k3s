using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShopManager.Interfaces
{
    public interface IEmailSmtpSender
    {
        Task SendEmail(string email, string subject, string message, string toUsername);
        Task<bool> ValidateSmtpAsync();
        bool ValidateSmtp(out string responseData);
    }
}
