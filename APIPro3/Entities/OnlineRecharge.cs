using System;
using System.Collections.Generic;

namespace APIPro3.Entities;

public partial class OnlineRecharge
{
    public int RechargeId { get; set; }

    public int? UserId { get; set; }

    public string? GuestIdentifier { get; set; }

    public decimal Amount { get; set; }

    public DateTime? RechargeDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual User? User { get; set; }
}
