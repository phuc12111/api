using System;
using System.Collections.Generic;

namespace APIPro3.Entities;

public partial class Invoice
{
    public string InvoiceId { get; set; } = null!;

    public decimal Amount { get; set; }

    public bool Status { get; set; }
}
