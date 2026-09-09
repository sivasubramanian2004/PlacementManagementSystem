using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Service.Email
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
