using System;
using System.Collections.Generic;

namespace APIPro3.Entities;

public partial class User
{
    public int UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string? Role { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<OnlineRecharge> OnlineRecharges { get; set; } = new List<OnlineRecharge>();

    public virtual ICollection<PostBillPayment> PostBillPayments { get; set; } = new List<PostBillPayment>();

    public virtual ICollection<RecordMiniGame> RecordMiniGames { get; set; } = new List<RecordMiniGame>();

    public virtual ICollection<TransactionLog> TransactionLogs { get; set; } = new List<TransactionLog>();
}
