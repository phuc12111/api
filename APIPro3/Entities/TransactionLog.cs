using System;
using System.Collections.Generic;

namespace APIPro3.Entities;

public partial class TransactionLog
{
    public int TransactionId { get; set; }

    public int? UserId { get; set; }

    public string? GuestIdentifier { get; set; }

    public string TransactionType { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime? TransactionDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual User? User { get; set; }
}
