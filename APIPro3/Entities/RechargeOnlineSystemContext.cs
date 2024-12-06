using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace APIPro3.Entities;

public partial class RechargeOnlineSystemContext : DbContext
{
    public RechargeOnlineSystemContext()
    {
    }

    public RechargeOnlineSystemContext(DbContextOptions<RechargeOnlineSystemContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<AccountRingtone> AccountRingtones { get; set; }

    public virtual DbSet<DateManage> DateManages { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<OnlineRecharge> OnlineRecharges { get; set; }

    public virtual DbSet<PostBillPayment> PostBillPayments { get; set; }

    public virtual DbSet<ReFeedback> ReFeedbacks { get; set; }

    public virtual DbSet<RecordMiniGame> RecordMiniGames { get; set; }

    public virtual DbSet<Ringtone> Ringtones { get; set; }

    public virtual DbSet<TransactionLog> TransactionLogs { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=Recharge_Online_System;Integrated Security=True;Encrypt=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__Account__46A222CDEE4E7A8E");

            entity.ToTable("Account");

            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.AccountBalance)
                .HasDefaultValue(0.00m)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("account_balance");
            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("last_updated");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Account__user_id__5AEE82B9");
        });

        modelBuilder.Entity<AccountRingtone>(entity =>
        {
            entity.HasKey(e => new { e.AccountId, e.RingtoneId }).HasName("PK__Account___067BB1DCABE66975");

            entity.ToTable("Account_Ringtone");

            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.RingtoneId).HasColumnName("ringtone_id");
            entity.Property(e => e.AccountRingtoneDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("account_ringtone_date");

            entity.HasOne(d => d.Account).WithMany(p => p.AccountRingtones)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("FK__Account_R__accou__619B8048");

            entity.HasOne(d => d.Ringtone).WithMany(p => p.AccountRingtones)
                .HasForeignKey(d => d.RingtoneId)
                .HasConstraintName("FK__Account_R__ringt__628FA481");
        });

        modelBuilder.Entity<DateManage>(entity =>
        {
            entity.HasKey(e => e.DateId).HasName("PK__DateMana__51FC48657617D727");

            entity.ToTable("DateManage");

            entity.Property(e => e.DateId).HasColumnName("date_id");
            entity.Property(e => e.DateSignin)
                .HasColumnType("datetime")
                .HasColumnName("date_signin");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__Feedback__7A6B2B8C1B3A5291");

            entity.ToTable("Feedback");

            entity.Property(e => e.FeedbackId).HasColumnName("feedback_id");
            entity.Property(e => e.FeedbackDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("feedback_date");
            entity.Property(e => e.FeedbackText)
                .HasColumnType("text")
                .HasColumnName("feedback_text");
            entity.Property(e => e.GuestIdentifier)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("guest_identifier");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Feedback__user_i__47DBAE45");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("PK__Invoice__D796AAB5D3A00882");

            entity.ToTable("Invoice");

            entity.Property(e => e.InvoiceId).HasMaxLength(50);
            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Status).HasColumnName("status");
        });

        modelBuilder.Entity<OnlineRecharge>(entity =>
        {
            entity.HasKey(e => e.RechargeId).HasName("PK__OnlineRe__D3CC5F61BDCFFCAD");

            entity.ToTable("OnlineRecharge");

            entity.Property(e => e.RechargeId).HasColumnName("recharge_id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.GuestIdentifier)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("guest_identifier");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.RechargeDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("recharge_date");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.OnlineRecharges)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__OnlineRec__user___3F466844");
        });

        modelBuilder.Entity<PostBillPayment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__PostBill__ED1FC9EAB282E14B");

            entity.ToTable("PostBillPayment");

            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.BillNumber)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("bill_number");
            entity.Property(e => e.GuestIdentifier)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("guest_identifier");
            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("payment_date");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.PostBillPayments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__PostBillP__user___440B1D61");
        });

        modelBuilder.Entity<ReFeedback>(entity =>
        {
            entity.HasKey(e => e.RefeedbackId).HasName("PK__ReFeedba__799F0EFBFFE33D46");

            entity.ToTable("ReFeedback");

            entity.Property(e => e.RefeedbackId).HasColumnName("refeedback_id");
            entity.Property(e => e.FeedbackId).HasColumnName("feedback_id");
            entity.Property(e => e.RefeedbackDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("refeedback_date");
            entity.Property(e => e.RefeedbackText)
                .HasColumnType("text")
                .HasColumnName("refeedback_text");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("status");

            entity.HasOne(d => d.Feedback).WithMany(p => p.ReFeedbacks)
                .HasForeignKey(d => d.FeedbackId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__ReFeedbac__feedb__4BAC3F29");
        });

        modelBuilder.Entity<RecordMiniGame>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("PK__RecordMi__BFCFB4DDD18E0949");

            entity.ToTable("RecordMiniGame");

            entity.Property(e => e.RecordId).HasColumnName("record_id");
            entity.Property(e => e.SpinTurn).HasColumnName("spin_turn");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.RecordMiniGames)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__RecordMin__user___4E88ABD4");
        });

        modelBuilder.Entity<Ringtone>(entity =>
        {
            entity.HasKey(e => e.RingtoneId).HasName("PK__Ringtone__0D993115C52AC1D7");

            entity.ToTable("Ringtone");

            entity.Property(e => e.RingtoneId).HasColumnName("ringtone_id");
            entity.Property(e => e.FileUrl)
                .HasColumnType("text")
                .HasColumnName("file_url");
            entity.Property(e => e.RingtoneDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("ringtone_date");
            entity.Property(e => e.RingtoneName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("ringtone_name");
        });

        modelBuilder.Entity<TransactionLog>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__Transact__85C600AFE94DCB75");

            entity.ToTable("TransactionLog");

            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.GuestIdentifier)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("guest_identifier");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.TransactionDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("transaction_date");
            entity.Property(e => e.TransactionType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("transaction_type");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.TransactionLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Transacti__user___5535A963");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__B9BE370FF6E56477");

            entity.HasIndex(e => e.Email, "UQ__Users__AB6E6164BAAAA508").IsUnique();

            entity.HasIndex(e => e.Phone, "UQ__Users__B43B145F7FADDD03").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(11)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.Role)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("role");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("user_name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
