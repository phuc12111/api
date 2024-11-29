using System;
using System.Collections.Generic;

namespace APIPro3.Entities;

public partial class PostBillPayment
{
    public int PaymentId { get; set; }

    public int? UserId { get; set; }

    public string? GuestIdentifier { get; set; }

    public string BillNumber { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual User? User { get; set; }
}
