using System;
using System.Collections.Generic;

namespace Uzuki_CyberBank.Models;

public partial class GameLevel
{
    public int LevelId { get; set; }

    public string LevelName { get; set; } = null!;

    public string? Description { get; set; }

    public int BaseReward { get; set; }

    public string AttackType { get; set; } = null!;

    public string RequiredDefense { get; set; } = null!;

    public virtual ICollection<PlayerProgress> PlayerProgresses { get; set; } = new List<PlayerProgress>();

    public virtual ICollection<SimulatedTransaction> SimulatedTransactions { get; set; } = new List<SimulatedTransaction>();
}
