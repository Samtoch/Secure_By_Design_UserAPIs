using System;
using System.Collections.Generic;
using System.Text;

namespace Securebydesign.Application.Services.AuthServices
{
    public class AuditLog
    {
        public string Event { get; set; } = default!;
        public Guid? UserId { get; set; }
        public string? Role { get; set; }
        public string? Endpoint { get; set; }
        public int Status { get; set; }
        public string? Ip { get; set; }
        public string? CorrelationId { get; set; }
    }
}
