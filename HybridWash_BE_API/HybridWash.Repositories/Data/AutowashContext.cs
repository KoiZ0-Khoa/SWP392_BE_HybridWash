using System;
using System.Collections.Generic;
using HybridWash.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace HybridWash.Repositories.Data;

public partial class AutowashContext : DbContext
{
    public AutowashContext(DbContextOptions<AutowashContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<ParkingReceipt> ParkingReceipts { get; set; }

    public virtual DbSet<PointLedger> PointLedgers { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<Reward> Rewards { get; set; }

    public virtual DbSet<RewardRedemption> RewardRedemptions { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<Staff> Staff { get; set; }

    public virtual DbSet<TimeSlot> TimeSlots { get; set; }

    public virtual DbSet<Vehicle> Vehicles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("PK__Bookings__73951ACD10088F5C");

            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.ActualWashTime).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.FinalPrice)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.GuestLicensePlate)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.GuestName).HasMaxLength(100);
            entity.Property(e => e.GuestPhone)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.GuestVehicleType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.IncidentImage1).HasMaxLength(500);
            entity.Property(e => e.IncidentImage2).HasMaxLength(500);
            entity.Property(e => e.OriginalPrice)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PromotionId).HasColumnName("PromotionID");
            entity.Property(e => e.ServiceId).HasColumnName("ServiceID");
            entity.Property(e => e.SlotId).HasColumnName("SlotID");
            entity.Property(e => e.StaffId).HasColumnName("StaffID");
            entity.Property(e => e.StaffNote).HasMaxLength(1000);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pending");
            entity.Property(e => e.VehicleId).HasColumnName("VehicleID");

            entity.HasOne(d => d.Customer).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__Bookings__Custom__60A75C0F");

            entity.HasOne(d => d.Promotion).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.PromotionId)
                .HasConstraintName("FK__Bookings__Promot__6383C8BA");

            entity.HasOne(d => d.Service).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Bookings__Servic__628FA481");

            entity.HasOne(d => d.Slot).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.SlotId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Bookings__SlotID__6477ECF3");

            entity.HasOne(d => d.Staff).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK__Bookings__StaffI__656C112C");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK__Bookings__Vehicl__619B8048");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__Customer__A4AE64B8D3A53BAE");

            entity.HasIndex(e => e.PhoneNumber, "UQ__Customer__85FB4E38B3548610").IsUnique();

            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CurrentPoints).HasDefaultValue(0);
            entity.Property(e => e.CurrentTier)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Member");
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.TotalSpent)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<ParkingReceipt>(entity =>
        {
            entity.HasKey(e => e.ReceiptId).HasName("PK__ParkingR__CC08C40049DB80A3");

            entity.HasIndex(e => e.BookingId, "UQ__ParkingR__73951ACEF79FFCEF").IsUnique();

            entity.Property(e => e.ReceiptId).HasColumnName("ReceiptID");
            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.IsCustomerLeaving).HasDefaultValue(false);
            entity.Property(e => e.CustomerSignature).HasColumnType("nvarchar(max)");
            entity.Property(e => e.IssueStaffId).HasColumnName("IssueStaffID");
            entity.Property(e => e.IssuedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Issued");
            entity.Property(e => e.VerifyStaffId).HasColumnName("VerifyStaffID");
            entity.Property(e => e.VerifiedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Booking).WithOne(p => p.ParkingReceipt)
                .HasForeignKey<ParkingReceipt>(d => d.BookingId)
                .HasConstraintName("FK__ParkingRe__Booki__72C60C4A");

            entity.HasOne(d => d.IssueStaff).WithMany(p => p.ParkingReceiptIssueStaffs)
                .HasForeignKey(d => d.IssueStaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ParkingRe__Issue__73BA3083");

            entity.HasOne(d => d.VerifyStaff).WithMany(p => p.ParkingReceiptVerifyStaffs)
                .HasForeignKey(d => d.VerifyStaffId)
                .HasConstraintName("FK__ParkingRe__Verif__74AE54BC");
        });

        modelBuilder.Entity<PointLedger>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__PointLed__55433A4B3195A8D1");

            entity.HasIndex(e => e.RewardRedemptionId)
                .IsUnique()
                .HasDatabaseName("UX_PointLedger_RewardRedemptionID")
                .HasFilter("[RewardRedemptionID] IS NOT NULL");

            entity.ToTable("PointLedger");

            entity.Property(e => e.TransactionId).HasColumnName("TransactionID");
            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ExpireDate).HasColumnType("datetime");
            entity.Property(e => e.RewardRedemptionId).HasColumnName("RewardRedemptionID");
            entity.Property(e => e.TransactionType)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Booking).WithMany(p => p.PointLedgers)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK__PointLedg__Booki__6C190EBB");

            entity.HasOne(d => d.Customer).WithMany(p => p.PointLedgers)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PointLedg__Custo__6B24EA82");

            entity.HasOne(d => d.RewardRedemption).WithOne(p => p.PointTransaction)
                .HasForeignKey<PointLedger>(d => d.RewardRedemptionId)
                .HasConstraintName("FK_PointLedger_RewardRedemptions");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.PromotionId).HasName("PK__Promotio__52C42F2F36596DAD");

            entity.HasIndex(e => e.PromoCode, "UQ__Promotio__32DBED3595140775").IsUnique();

            entity.Property(e => e.PromotionId).HasColumnName("PromotionID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PromoCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PromoName).HasMaxLength(100);
            entity.Property(e => e.PromoType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TargetTier)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ValidFrom).HasColumnType("datetime");
            entity.Property(e => e.ValidTo).HasColumnType("datetime");
        });

        modelBuilder.Entity<Reward>(entity =>
        {
            entity.HasKey(e => e.RewardId);

            entity.HasIndex(e => e.RewardName)
                .IsUnique()
                .HasDatabaseName("UQ_Rewards_RewardName");

            entity.Property(e => e.RewardId).HasColumnName("RewardID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DiscountValue).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MinimumTier)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Member");
            entity.Property(e => e.RewardName).HasMaxLength(100);
            entity.Property(e => e.RewardType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ServiceId).HasColumnName("ServiceID");
            entity.Property(e => e.ValidFrom).HasColumnType("datetime");
            entity.Property(e => e.ValidTo).HasColumnType("datetime");

            entity.HasOne(d => d.Service).WithMany()
                .HasForeignKey(d => d.ServiceId)
                .HasConstraintName("FK_Rewards_Services");
        });

        modelBuilder.Entity<RewardRedemption>(entity =>
        {
            entity.HasKey(e => e.RedemptionId);

            entity.HasIndex(e => e.RequestId)
                .IsUnique()
                .HasDatabaseName("UQ_RewardRedemptions_RequestId");

            entity.Property(e => e.RedemptionId).HasColumnName("RedemptionID");
            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.RedeemedAt).HasColumnType("datetime");
            entity.Property(e => e.RewardId).HasColumnName("RewardID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Issued");
            entity.Property(e => e.UsedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Booking).WithMany()
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK_RewardRedemptions_Bookings");

            entity.HasOne(d => d.Customer).WithMany(p => p.RewardRedemptions)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RewardRedemptions_Customers");

            entity.HasOne(d => d.Reward).WithMany(p => p.Redemptions)
                .HasForeignKey(d => d.RewardId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RewardRedemptions_Rewards");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("PK__Services__C51BB0EAB7B79E7F");

            entity.Property(e => e.ServiceId).HasColumnName("ServiceID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ServiceName).HasMaxLength(100);
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.StaffId).HasName("PK__Staff__96D4AAF7BC695691");

            entity.HasIndex(e => e.PhoneNumber, "UQ__Staff__85FB4E3850FF2E93").IsUnique();

            entity.Property(e => e.StaffId).HasColumnName("StaffID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Washer");
        });

        modelBuilder.Entity<TimeSlot>(entity =>
        {
            entity.HasKey(e => e.SlotId).HasName("PK__TimeSlot__0A124A4FD93E363B");

            entity.Property(e => e.SlotId).HasColumnName("SlotID");
            entity.Property(e => e.BikeCapacity).HasDefaultValue(5);
            entity.Property(e => e.CarCapacity).HasDefaultValue(2);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.VehicleId).HasName("PK__Vehicles__476B54B21101B620");

            entity.HasIndex(e => e.LicensePlate, "UQ__Vehicles__026BC15CABA5DDE3").IsUnique();

            entity.Property(e => e.VehicleId).HasColumnName("VehicleID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.LicensePlate)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.VehicleType).HasMaxLength(50);

            entity.HasOne(d => d.Customer).WithMany(p => p.Vehicles)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Vehicles__Custom__4222D4EF");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
