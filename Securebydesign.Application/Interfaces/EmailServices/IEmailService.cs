using System;
using System.Collections.Generic;
using System.Text;

namespace Securebydesign.Application.Interfaces.EmailServices
{
    public interface IEmailService
    {
        Task<bool> SendSignupEmail(string recipientEmail, string recipientName, string token);
        Task<bool> SendForgotPasswordEmail(string recipientEmail, string recipientName, string token);
        Task<bool> SendResetPasswordEmail(string recipientEmail, string recipientName);
        Task<bool> SendFreshTokenEmail(string recipientEmail, string recipientName, string token, string purpose);
        //Task<bool> SendFeedNotificationEmail(
        //    string recipientEmail,
        //    string recipientName,
        //    string title,
        //    string body,
        //    string postLink,
        //    string? imageUrl,
        //    List<string> tags);

        // IEmailService — updated signature
    }
}
