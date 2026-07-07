using System;
using System.Collections.Generic;

namespace Uzuki_CyberBank.Models;

public partial class AuditLog
{
    public Guid LogId { get; set; }

    public Guid? UserId { get; set; }

    public string ActionType { get; set; } = null!;

    public string LogDetails { get; set; } = null!;

    public string? IpAddress { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
