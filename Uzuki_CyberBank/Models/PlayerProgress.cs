using System;
using System.Collections.Generic;

namespace Uzuki_CyberBank.Models;

public partial class PlayerProgress
{
    public Guid ProgressId { get; set; }

    public Guid UserId { get; set; }

    public int LevelId { get; set; }

    public int ScoreEarned { get; set; }

    public DateTime CompletedAt { get; set; }

    public virtual GameLevel Level { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
