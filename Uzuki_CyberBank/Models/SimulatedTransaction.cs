using System;
using System.Collections.Generic;

namespace Uzuki_CyberBank.Models;

public partial class SimulatedTransaction
{
    public Guid TransactionId { get; set; }

    public int LevelId { get; set; }

    public Guid SenderAccountId { get; set; }

    public Guid ReceiverAccountId { get; set; }

    public decimal OriginalAmount { get; set; }

    public string? EncryptedPayload { get; set; }

    public string? Nonce { get; set; }

    public DateTime? Timestamp { get; set; }

    public string? Signature { get; set; }

    public bool IsHacked { get; set; }

    public bool? IsBlockedByPlayer { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual GameLevel Level { get; set; } = null!;

    public virtual BankAccount ReceiverAccount { get; set; } = null!;

    public virtual BankAccount SenderAccount { get; set; } = null!;
}
