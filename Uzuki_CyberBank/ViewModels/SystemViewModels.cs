using System;

namespace Uzuki_CyberBank.ViewModels
{
    public class AuditLogViewModel
    {
        public string? Username { get; set; }
        public string? ActionType { get; set; }
        public string? LogDetails { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}