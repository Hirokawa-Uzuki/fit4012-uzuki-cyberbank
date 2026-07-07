using System;
using System.Collections.Generic;

namespace Uzuki_CyberBank.Models;

public partial class User
{
    public Guid UserId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int Score { get; set; }

    public int CurrentLevel { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<PlayerProgress> PlayerProgresses { get; set; } = new List<PlayerProgress>();
}
