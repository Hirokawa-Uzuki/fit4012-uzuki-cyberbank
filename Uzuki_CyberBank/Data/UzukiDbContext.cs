using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Uzuki_CyberBank.Models;

namespace Uzuki_CyberBank.Data;

public partial class UzukiDbContext : DbContext
{
    public UzukiDbContext()
    {
    }

    public UzukiDbContext(DbContextOptions<UzukiDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<BankAccount> BankAccounts { get; set; }

    public virtual DbSet<GameLevel> GameLevels { get; set; }

    public virtual DbSet<PlayerProgress> PlayerProgresses { get; set; }

    public virtual DbSet<SimulatedTransaction> SimulatedTransactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        //=> optionsBuilder.UseSqlServer("Server=.;Database=Uzuki_CB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__AuditLog__5E5486483228A221");

            entity.Property(e => e.LogId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ActionType).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Audit_User");
        });

        modelBuilder.Entity<BankAccount>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__BankAcco__349DA5A6C43822AE");

            entity.HasIndex(e => e.AccountNumber, "UQ__BankAcco__BE2ACD6F1DAED35E").IsUnique();

            entity.Property(e => e.AccountId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AccountName).HasMaxLength(100);
            entity.Property(e => e.AccountNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Balance).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<GameLevel>(entity =>
        {
            entity.HasKey(e => e.LevelId).HasName("PK__GameLeve__09F03C268A34C55A");

            entity.Property(e => e.LevelId).ValueGeneratedNever();
            entity.Property(e => e.AttackType).HasMaxLength(50);
            entity.Property(e => e.BaseReward).HasDefaultValue(100);
            entity.Property(e => e.LevelName).HasMaxLength(100);
            entity.Property(e => e.RequiredDefense).HasMaxLength(50);
        });

        modelBuilder.Entity<PlayerProgress>(entity =>
        {
            entity.HasKey(e => e.ProgressId).HasName("PK__PlayerPr__BAE29CA5109B4D5B");

            entity.ToTable("PlayerProgress");

            entity.Property(e => e.ProgressId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CompletedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Level).WithMany(p => p.PlayerProgresses)
                .HasForeignKey(d => d.LevelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Progress_Level");

            entity.HasOne(d => d.User).WithMany(p => p.PlayerProgresses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Progress_User");
        });

        modelBuilder.Entity<SimulatedTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__Simulate__55433A6B818F7AD0");

            entity.Property(e => e.TransactionId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Nonce)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OriginalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Timestamp).HasColumnType("datetime");

            entity.HasOne(d => d.Level).WithMany(p => p.SimulatedTransactions)
                .HasForeignKey(d => d.LevelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SimTx_Level");

            entity.HasOne(d => d.ReceiverAccount).WithMany(p => p.SimulatedTransactionReceiverAccounts)
                .HasForeignKey(d => d.ReceiverAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SimTx_Receiver");

            entity.HasOne(d => d.SenderAccount).WithMany(p => p.SimulatedTransactionSenderAccounts)
                .HasForeignKey(d => d.SenderAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SimTx_Sender");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CEAB22C06");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E4C8097F28").IsUnique();

            entity.Property(e => e.UserId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CurrentLevel).HasDefaultValue(1);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
