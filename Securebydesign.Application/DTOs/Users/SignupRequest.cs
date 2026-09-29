using System.ComponentModel.DataAnnotations;

namespace Securebydesign.Application.DTOs.Users
{
    public class SignupRequest
    {
        [Required, EmailAddress, StringLength(254)]
        public required string Email { get; set; }

        // OWASP ASVS: at least 8 characters, allow long passphrases.
        // The upper limit stops oversized payloads being sent to the hasher.
        [Required, StringLength(128, MinimumLength = 8)]
        public required string Password { get; set; }

        [Required, StringLength(100)]
        public required string FirstName { get; set; }

        [Required, StringLength(100)]
        public required string LastName { get; set; }

        [Phone, StringLength(20)]
        public string? PhoneNumber { get; set; }
    }
}
