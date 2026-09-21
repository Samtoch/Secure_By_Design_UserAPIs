
using Securebydesign.Application.Interfaces.EmailTemplate;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Securebydesign.Infrastructure.EmailTemplates
{
    public class EmailTemplate : IEmailTemplate
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailTemplate> _logger;
        private readonly string emailTemplate;

        public EmailTemplate(ILogger<EmailTemplate> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            emailTemplate = _configuration["Brevo:EmailTemplates"] ?? @"/var/www/emailtemplates";
        }
        public async Task<string> GenerateSignupEmailBody(string name, string link)
        {
            try
            {
                // 1. Trim whitespace/newlines and normalize slashes
                string templateDirectory = (emailTemplate ?? "").Trim().TrimEnd('/', '\\');
                string templatePath = Path.Combine(templateDirectory, "Signup.html");

                // 2. Wrap in single quotes in logs to spot hidden trailing spaces!
                _logger.LogInformation("Looking for email template at path: '{Path}'", templatePath);

                if (File.Exists(templatePath))
                {
                    string templateContent = await File.ReadAllTextAsync(templatePath);

                    return templateContent
                        .Replace("{{name}}", name)
                        .Replace("{{link}}", link);
                }

                // If File.Exists returns false, list all files in directory to diagnose
                if (Directory.Exists(templateDirectory))
                {
                    var files = Directory.GetFiles(templateDirectory);
                    _logger.LogWarning("Template not found. Directory '{Dir}' contains files: {Files}",
                        templateDirectory, string.Join(", ", files));
                }
                else
                {
                    _logger.LogWarning("Template directory '{Dir}' DOES NOT EXIST according to .NET!", templateDirectory);
                }

                return $"<html><body style='font-family:sans-serif;'><h1>Welcome {name}</h1><p>Please verify your account to continue: <a href='{link}'>{link}</a></p></body></html>";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while reading email template");
                return $"Welcome {name}. Please verify your account using this link: {link}";
            }
        }

        public async Task<string> GenerateForgotPasswordEmailBody(string name, string link)
        {
            try
            {
                string templateDirectory = emailTemplate;
                string templatePath = Path.Combine(templateDirectory, "ForgottenPassword.html");
                _logger.LogInformation("Looking for email template at path: {Path}", templatePath);

                if (File.Exists(templatePath))
                {
                    string templateContent = File.ReadAllText(templatePath);

                    // Replace the placeholders inside the ForgottenPassword.html file
                    return templateContent
                        .Replace("{{name}}", name)
                        .Replace("{{link}}", link);
                }

                // If the absolute path doesn't exist, log the specific error
                _logger.LogWarning("Email template not found at absolute path: {Path}. Using emergency fallback HTML.", templatePath);

                // Robust fallback if the file is missing from the drive
                return $"<html><body style='font-family:sans-serif;'><h1>Welcome {name}</h1><p>Please verify your account to continue: <a href='{link}'>{link}</a></p></body></html>";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while reading email template from {emailTemplate}", emailTemplate);
                return $"Welcome {name}. Please verify your account using this link: {link}";
            }
        }

        public async Task<string> GenerateResetPasswordEmailBody(string email, string name)
        {
            try
            {
                string templateDirectory = emailTemplate;
                string templatePath = Path.Combine(templateDirectory, "ResetPassword.html");
                _logger.LogInformation("Looking for email template at path: {Path}", templatePath);

                if (File.Exists(templatePath))
                {
                    string templateContent = File.ReadAllText(templatePath);

                    // Replace the placeholders inside the ResetPassword.html file
                    return templateContent
                        .Replace("{{name}}", name)
                        .Replace("{{email}}", email);
                }

                // If the absolute path doesn't exist, log the specific error
                _logger.LogWarning("Email template not found at absolute path: {Path}. Using emergency fallback HTML.", templatePath);

                // Robust fallback if the file is missing from the drive
                return $"<html><body style='font-family:sans-serif;'><h1>Welcome {name}</h1><p>Please login to your account to continue.</a></p></body></html>";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while reading email template from {emailTemplate}", emailTemplate);
                return $"Welcome {name}. Please login to your account to continue.";
            }
        }

        public async Task<string> GenerateFreshTokenEmailBody(string name, string link, string purpose)
        {
            try
            {
                // 1. Trim whitespace/newlines and normalize slashes
                string templateDirectory = (emailTemplate ?? "").Trim().TrimEnd('/', '\\');
                string templatePath = Path.Combine(templateDirectory, "FreshToken.html");

                // 2. Wrap in single quotes in logs to spot hidden trailing spaces!
                _logger.LogInformation("Looking for email template at path: '{Path}'", templatePath);

                if (File.Exists(templatePath))
                {
                    string templateContent = await File.ReadAllTextAsync(templatePath);

                    return templateContent
                        .Replace("{{name}}", name)
                        .Replace("{{link}}", link)
                        .Replace("{{purpose}}", purpose);
                }

                // If File.Exists returns false, list all files in directory to diagnose
                if (Directory.Exists(templateDirectory))
                {
                    var files = Directory.GetFiles(templateDirectory);
                    _logger.LogWarning("Template not found. Directory '{Dir}' contains files: {Files}",
                        templateDirectory, string.Join(", ", files));
                }
                else
                {
                    _logger.LogWarning("Template directory '{Dir}' DOES NOT EXIST according to .NET!", templateDirectory);
                }

                return $"<html><body style='font-family:sans-serif;'><h1>Welcome {name}</h1><p>Please verify your account to continue: <a href='{link}'>{link}</a></p></body></html>";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while reading email template");
                return $"Welcome {name}. Please verify your account using this link: {link}";
            }
        }

        //public async Task<string> GenerateFeedNotificationEmailBody(string recipientName, string title, string body, string postLink, string? imageUrl, List<string> tags)
        //{
        //    try
        //    {
        //        string templateDirectory = (emailTemplate ?? "").Trim().TrimEnd('/', '\\');
        //        string templatePath = Path.Combine(templateDirectory, "FeedNotification.html");

        //        _logger.LogInformation(
        //            "Looking for feed notification template at: '{Path}'", templatePath);

        //        string templateContent;

        //        if (File.Exists(templatePath))
        //        {
        //            templateContent = await File.ReadAllTextAsync(templatePath);
        //        }
        //        else
        //        {
        //            _logger.LogWarning(
        //                "FeedNotification.html not found at '{Path}'. Using fallback.", templatePath);

        //            templateContent = @"
        //        <html><body style='font-family:sans-serif;'>
        //        <h2>{{title}}</h2>
        //        <p>Hi {{name}},</p>
        //        <p>{{body}}</p>
        //        {{mediablock}}
        //        {{tagsblock}}
        //        <p><a href='{{postlink}}'>View Post</a></p>
        //        </body></html>";
        //        }

        //        // Build optional media block
        //        var mediaBlock = string.IsNullOrWhiteSpace(imageUrl)
        //            ? string.Empty
        //            : $@"<div style='text-align:center; margin: 20px 0;'>
        //            <img src='{imageUrl}' alt='Post image'
        //                 style='max-width:100%; border-radius:8px;'/>
        //         </div>";

        //        // Build optional tags block
        //        var tagsBlock = tags.Any()
        //            ? $@"<div style='margin: 20px 0;'>
        //            <p style='font-size:13px; color:#64748b; margin-bottom:8px;'>Topics:</p>
        //            <div>{string.Join(" ", tags.Select(t =>
        //                        $"<span style='display:inline-block; background:#ede9fe; color:#5b21b6; " +
        //                        $"padding:3px 10px; border-radius:999px; font-size:12px; margin:3px;'>#{t}</span>"))}</div>
        //         </div>"
        //            : string.Empty;

        //        return templateContent
        //            .Replace("{{name}}", recipientName)
        //            .Replace("{{title}}", title)
        //            .Replace("{{body}}", body)
        //            .Replace("{{postlink}}", postLink)
        //            .Replace("{{mediablock}}", mediaBlock)
        //            .Replace("{{tagsblock}}", tagsBlock);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error generating feed notification email body");
        //        return $"<html><body><h2>{title}</h2><p>Hi {recipientName},</p><p>{body}</p>" +
        //               $"<p><a href='{postLink}'>View Post</a></p></body></html>";
        //    }
        //}
    }
}
