using System;
using System.Collections.Generic;

namespace APIPro3.Entities;

public partial class AccountRingtone
{
    public int AccountId { get; set; }

    public int RingtoneId { get; set; }

    public DateTime? AccountRingtoneDate { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Ringtone Ringtone { get; set; } = null!;
}
