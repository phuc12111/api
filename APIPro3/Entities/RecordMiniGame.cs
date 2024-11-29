using System;
using System.Collections.Generic;

namespace APIPro3.Entities;

public partial class RecordMiniGame
{
    public int RecordId { get; set; }

    public int? UserId { get; set; }

    public int? SpinTurn { get; set; }

    public virtual User? User { get; set; }
}
