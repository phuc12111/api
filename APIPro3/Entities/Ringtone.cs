using System;
using System.Collections.Generic;

namespace APIPro3.Entities;

public partial class Ringtone
{
    public int RingtoneId { get; set; }

    public string RingtoneName { get; set; } = null!;

    public string FileUrl { get; set; } = null!;

    public DateTime? RingtoneDate { get; set; }

    public virtual ICollection<AccountRingtone> AccountRingtones { get; set; } = new List<AccountRingtone>();
}
