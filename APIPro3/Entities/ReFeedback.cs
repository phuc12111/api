using System;
using System.Collections.Generic;

namespace APIPro3.Entities;

public partial class ReFeedback
{
    public int RefeedbackId { get; set; }

    public int? FeedbackId { get; set; }

    public string RefeedbackText { get; set; } = null!;

    public DateTime? RefeedbackDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual Feedback? Feedback { get; set; }
}
