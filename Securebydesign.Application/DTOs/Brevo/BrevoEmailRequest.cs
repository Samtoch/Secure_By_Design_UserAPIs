using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Securebydesign.Application.DTOs.Brevo
{
    public class BrevoEmailRequest
    {
        [JsonPropertyName("sender")]
        public EmailPerson Sender { get; set; }

        [JsonPropertyName("to")]
        public List<EmailPerson> To { get; set; }

        [JsonPropertyName("subject")]
        public string Subject { get; set; }

        [JsonPropertyName("htmlContent")]
        public string HtmlContent { get; set; }
    }
}
