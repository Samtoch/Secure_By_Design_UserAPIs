using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Securebydesign.Application.DTOs.Users
{
    public class UpdateRequest
    {
        [EmailAddress, StringLength(254)]
        public string? Username { get; set; }

        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        [Phone, StringLength(20)]
        public string? PhoneNumber { get; set; }

        // Nullable so an omitted value doesn't silently become 0
        [Range(1, int.MaxValue)]
        public int? NeighborhoodId { get; set; }

        [StringLength(100)]
        public string? City { get; set; }
    }
}
