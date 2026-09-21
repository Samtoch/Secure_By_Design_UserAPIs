using System;
using System.Collections.Generic;
using System.Text;

namespace Securebydesign.Application.Interfaces.EmailTemplate
{
    public interface IEmailTemplate
    {
        Task<string> GenerateSignupEmailBody(string name, string link);
        Task<string> GenerateForgotPasswordEmailBody(string name, string link);
        Task<string> GenerateResetPasswordEmailBody(string email, string name);
        Task<string> GenerateFreshTokenEmailBody(string name, string link, string purpose);
        //Task<string> GenerateFeedNotificationEmailBody(
        //    string recipientName,
        //    string title,
        //    string body,
        //    string postLink,
        //    string? imageUrl,
        //    List<string> tags);


    }
}
