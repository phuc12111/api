using System;
using System.Collections.Generic;

namespace APIPro3.Entities;

public partial class Feedback
{
    public int FeedbackId { get; set; }

    public int? UserId { get; set; }

    public string? GuestIdentifier { get; set; }

    public string FeedbackText { get; set; } = null!;

    public DateTime? FeedbackDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<ReFeedback> ReFeedbacks { get; set; } = new List<ReFeedback>();

    public virtual User? User { get; set; }
}
