using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Securebydesign.Application.DTOs.Brevo
{
    public class EmailPerson
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }
    }
}
