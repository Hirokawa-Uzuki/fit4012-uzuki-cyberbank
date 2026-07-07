using System;
using System.Collections.Generic;

namespace Uzuki_CyberBank.Models;

public partial class BankAccount
{
    public Guid AccountId { get; set; }

    public string AccountName { get; set; } = null!;

    public string AccountNumber { get; set; } = null!;

    public decimal Balance { get; set; }

    public string? PublicKey { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<SimulatedTransaction> SimulatedTransactionReceiverAccounts { get; set; } = new List<SimulatedTransaction>();

    public virtual ICollection<SimulatedTransaction> SimulatedTransactionSenderAccounts { get; set; } = new List<SimulatedTransaction>();
}
