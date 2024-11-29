using System;
using System.Collections.Generic;

namespace APIPro3.Entities;

public partial class Account
{
    public int AccountId { get; set; }

    public int UserId { get; set; }

    public decimal? AccountBalance { get; set; }

    public DateTime? LastUpdated { get; set; }

    public virtual ICollection<AccountRingtone> AccountRingtones { get; set; } = new List<AccountRingtone>();

    public virtual User? User { get; set; }
}
