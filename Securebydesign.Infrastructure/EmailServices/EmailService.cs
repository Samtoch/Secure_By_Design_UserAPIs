using Securebydesign.Application.DTOs.Brevo;
using Securebydesign.Application.Interfaces;
using Securebydesign.Application.Interfaces.EmailServices;
using Securebydesign.Application.Interfaces.EmailTemplate;
using Securebydesign.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Securebydesign.Infrastructure.EmailServices
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IEmailTemplate _emailTemplate;
        private readonly IConfiguration _configuration;
        private readonly IHttpRequests _httpRequests;
        private readonly string emailEndpoint, baseUrl, domain, apiKey, apiValue, senderName, senderEmail, emailDirectory;
        public EmailService(ILogger<EmailService> logger, IEmailTemplate emailTemplate, IConfiguration configuration, IHttpRequests httpRequests)
        {
            _logger = logger;
            _httpRequests = httpRequests;
            _configuration = configuration;
            _emailTemplate = emailTemplate;
            domain = _configuration["Domain"] ?? string.Empty; //updated domain name in appsettings
            baseUrl = _configuration["Brevo:BaseUrl"] ?? string.Empty;
            apiKey = _configuration["Brevo:ApiKey"] ?? string.Empty;
            apiValue = _configuration["Brevo:Value"] ?? string.Empty;
            emailEndpoint = _configuration["Brevo:Email"] ?? string.Empty;
            emailDirectory = _configuration["Brevo:EmailTemplates"] ?? @"C:\EmailTemplates\Scantrue";
            senderName = _configuration["Brevo:SenderName"] ?? string.Empty;
            senderEmail = _configuration["Brevo:SenderEmail"] ?? string.Empty;
        }

        public async Task<bool> SendSignupEmail(string recipientEmail, string recipientName, string token)
        {
            _logger.LogInformation("Preparing to send signup email to {Email}", recipientEmail);

            string subject = "Welcome to Ashuam";

            string verificationLink = $"{domain}/User/Verify?token={token}";
            _logger.LogInformation("Generated verification link for {Email}: {Link}", recipientEmail, verificationLink);

            string htmlBody = await _emailTemplate.GenerateSignupEmailBody(recipientName, verificationLink);

            var payload = new BrevoEmailRequest
            {
                Sender = new EmailPerson() { Name = senderName, Email = senderEmail },
                To = new List<EmailPerson>() { new EmailPerson { Email = recipientEmail, Name = recipientName } },
                Subject = subject,
                HtmlContent = htmlBody
            };

            var authOptions = new AuthOptions
            {
                AuthType = AuthType.ApiKey,
                KeyOrName = apiKey,
                Value = apiValue,
            };

            try
            {
                var response = await _httpRequests.PostAsync(baseUrl, emailEndpoint, payload, authOptions);

                if (!string.IsNullOrEmpty(response))
                {
                    var json = JObject.Parse(response);

                    // Brevo success returns a "messageId"
                    if (json["messageId"] != null)
                    {
                        _logger.LogInformation("Email DELIVERED to {Email}. Brevo MessageId: {MessageId}", recipientEmail, json["messageId"].ToString());
                    }
                    // Brevo failure returns a "message" and "code"
                    else if (json["message"] != null)
                    {
                        _logger.LogWarning("Email FAILED for {Email}. Reason: {Reason} (Code: {Code})", recipientEmail, json["message"].ToString(), json["code"]?.ToString());
                    }
                }
                else
                {
                    _logger.LogError("Email FAILED for {Email}. Received an empty response from Brevo.", recipientEmail);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error occurred while sending signup email to {Email}", recipientEmail);
                return false;
            }
        }

        public async Task<bool> SendForgotPasswordEmail(string recipientEmail, string recipientName, string token)
        {
            _logger.LogInformation("Preparing to send forgot password email to {Email}", recipientEmail);
            
            string subject = "Reset your password for Ashuam";

            //string verificationLink = $"{domain}/Home/ResetPassword?token={token}";

            string htmlBody = await _emailTemplate.GenerateForgotPasswordEmailBody(recipientName, token);

            var payload = new BrevoEmailRequest
            {
                Sender = new EmailPerson() { Name = senderName, Email = senderEmail },
                To = new List<EmailPerson>() { new EmailPerson { Email = recipientEmail, Name = recipientName } },
                Subject = subject,
                HtmlContent = htmlBody
            };

            var authOptions = new AuthOptions
            {
                AuthType = AuthType.ApiKey,
                KeyOrName = apiKey,
                Value = apiValue,
            };

            try
            {
                var response = await _httpRequests.PostAsync(baseUrl, emailEndpoint, payload, authOptions);

                if (!string.IsNullOrEmpty(response))
                {
                    var json = JObject.Parse(response);

                    // Brevo success returns a "messageId"
                    if (json["messageId"] != null)
                    {
                        _logger.LogInformation("Email DELIVERED to {Email}. Brevo MessageId: {MessageId}", recipientEmail, json["messageId"].ToString());
                    }
                    // Brevo failure returns a "message" and "code"
                    else if (json["message"] != null)
                    {
                        _logger.LogWarning("Email FAILED for {Email}. Reason: {Reason} (Code: {Code})", recipientEmail, json["message"].ToString(), json["code"]?.ToString());
                    }
                }
                else
                {
                    _logger.LogError("Email FAILED for {Email}. Received an empty response from Brevo.", recipientEmail);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error occurred while sending signup email to {Email}", recipientEmail);
                return false;
            }
        }

        public async Task<bool> SendResetPasswordEmail(string recipientEmail, string recipientName)
        {
            _logger.LogInformation("Preparing to send reset password email to {Email}", recipientEmail);
            string subject = "Password reset successful";

            string htmlBody = await _emailTemplate.GenerateResetPasswordEmailBody(recipientEmail, recipientName);

            var payload = new BrevoEmailRequest
            {
                Sender = new EmailPerson() { Name = senderName, Email = senderEmail },
                To = new List<EmailPerson>() { new EmailPerson { Email = recipientEmail, Name = recipientName } },
                Subject = subject,
                HtmlContent = htmlBody
            };

            var authOptions = new AuthOptions
            {
                AuthType = AuthType.ApiKey,
                KeyOrName = apiKey,
                Value = apiValue,
            };

            try
            {
                var response = await _httpRequests.PostAsync(baseUrl, emailEndpoint, payload, authOptions);

                if (!string.IsNullOrEmpty(response))
                {
                    var json = JObject.Parse(response);

                    // Brevo success returns a "messageId"
                    if (json["messageId"] != null)
                    {
                        _logger.LogInformation("Email DELIVERED to {Email}. Brevo MessageId: {MessageId}", recipientEmail, json["messageId"].ToString());
                    }
                    // Brevo failure returns a "message" and "code"
                    else if (json["message"] != null)
                    {
                        _logger.LogWarning("Email FAILED for {Email}. Reason: {Reason} (Code: {Code})", recipientEmail, json["message"].ToString(), json["code"]?.ToString());
                    }
                }
                else
                {
                    _logger.LogError("Email FAILED for {Email}. Received an empty response from Brevo.", recipientEmail);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error occurred while sending signup email to {Email}", recipientEmail);
                return  false;
            }
        }

        public async Task<bool> SendFreshTokenEmail(string recipientEmail, string recipientName, string token, string purpose)
        {
            _logger.LogInformation("Preparing to send new token email to {Email}", recipientEmail);

            string subject = $"Your {purpose} Request for Ashuam";

            string verificationLink = $"{domain}/User/Verify?token={token}";
            _logger.LogInformation("Generated verification link for {Email}: {Link}", recipientEmail, verificationLink);

            string htmlBody = await _emailTemplate.GenerateFreshTokenEmailBody(recipientName, verificationLink, purpose);

            var payload = new BrevoEmailRequest
            {
                Sender = new EmailPerson() { Name = senderName, Email = senderEmail },
                To = new List<EmailPerson>() { new EmailPerson { Email = recipientEmail, Name = recipientName } },
                Subject = subject,
                HtmlContent = htmlBody
            };

            var authOptions = new AuthOptions
            {
                AuthType = AuthType.ApiKey,
                KeyOrName = apiKey,
                Value = apiValue,
            };

            try
            {
                var response = await _httpRequests.PostAsync(baseUrl, emailEndpoint, payload, authOptions);

                if (!string.IsNullOrEmpty(response))
                {
                    var json = JObject.Parse(response);

                    // Brevo success returns a "messageId"
                    if (json["messageId"] != null)
                    {
                        _logger.LogInformation("Email DELIVERED to {Email}. Brevo MessageId: {MessageId}", recipientEmail, json["messageId"].ToString());
                    }
                    // Brevo failure returns a "message" and "code"
                    else if (json["message"] != null)
                    {
                        _logger.LogWarning("Email FAILED for {Email}. Reason: {Reason} (Code: {Code})", recipientEmail, json["message"].ToString(), json["code"]?.ToString());
                    }
                }
                else
                {
                    _logger.LogError("Email FAILED for {Email}. Received an empty response from Brevo.", recipientEmail);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error occurred while sending signup email to {Email}", recipientEmail);
                return false;
            }
        }

    }
}
