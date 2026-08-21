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

    public virtual DbSet<BookingAddOn> BookingAddOns { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<CustomerTierHistory> CustomerTierHistories { get; set; }

    public virtual DbSet<ParkingReceipt> ParkingReceipts { get; set; }

    public virtual DbSet<PointLedger> PointLedgers { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<Reward> Rewards { get; set; }

    public virtual DbSet<RewardRedemption> RewardRedemptions { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<Staff> Staff { get; set; }

    public virtual DbSet<TimeSlot> TimeSlots { get; set; }

    public virtual DbSet<TierRule> TierRules { get; set; }

    public virtual DbSet<Vehicle> Vehicles { get; set; }

    public virtual DbSet<SystemParameter> SystemParameters { get; set; }
    public virtual DbSet<IncidentReport> IncidentReports { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("PK__Bookings__73951ACD7262D128");

            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.ActualWashTime).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.FinalPrice)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DepositAmount)
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
            entity.Property(e => e.IncidentImage3).HasMaxLength(500);
            entity.Property(e => e.IncidentImage4).HasMaxLength(500);
            entity.Property(e => e.IncidentImage5).HasMaxLength(500);
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
            entity.Property(e => e.QrCode)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.HasIndex(e => e.QrCode)
                .IsUnique()
                .HasFilter("[QrCode] IS NOT NULL");
            entity.Property(e => e.VehicleId).HasColumnName("VehicleID");

            entity.HasOne(d => d.Customer).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__Bookings__Custom__5DCAEF64");

            entity.HasOne(d => d.Promotion).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.PromotionId)
                .HasConstraintName("FK__Bookings__Promot__60A75C0F");

            entity.HasOne(d => d.Service).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Bookings__Servic__5FB337D6");

            entity.HasOne(d => d.Slot).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.SlotId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Bookings__SlotID__619B8048");

            entity.HasOne(d => d.Staff).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK__Bookings__StaffI__628FA481");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK__Bookings__Vehicl__5EBF139D");
        });

        modelBuilder.Entity<BookingAddOn>(entity =>
        {
            entity.HasKey(e => e.BookingAddOnId);

            entity.Property(e => e.BookingAddOnId).HasColumnName("BookingAddOnID");
            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.ServiceId).HasColumnName("ServiceID");
            entity.Property(e => e.PromotionId).HasColumnName("PromotionID");
            entity.Property(e => e.RedemptionId).HasColumnName("RedemptionID");
            entity.Property(e => e.OriginalPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.FinalPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pending");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasIndex(e => e.BookingId, "IX_BookingAddOns_BookingID");
            entity.HasIndex(e => e.ServiceId, "IX_BookingAddOns_ServiceID");
            entity.HasIndex(e => e.PromotionId, "IX_BookingAddOns_PromotionID");
            entity.HasIndex(e => e.RedemptionId, "UX_BookingAddOns_RedemptionID")
                .IsUnique()
                .HasFilter("[RedemptionID] IS NOT NULL");

            entity.HasOne(e => e.Booking).WithMany(e => e.BookingAddOns)
                .HasForeignKey(e => e.BookingId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_BookingAddOns_Bookings");

            entity.HasOne(e => e.Service).WithMany(e => e.BookingAddOns)
                .HasForeignKey(e => e.ServiceId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_BookingAddOns_Services");

            entity.HasOne(e => e.Promotion).WithMany(e => e.BookingAddOns)
                .HasForeignKey(e => e.PromotionId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_BookingAddOns_Promotions");

            entity.HasOne(e => e.Redemption).WithOne(e => e.BookingAddOn)
                .HasForeignKey<BookingAddOn>(e => e.RedemptionId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_BookingAddOns_RewardRedemptions");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__Customer__A4AE64B80ED0B1AD");

            entity.HasIndex(e => e.PhoneNumber, "UQ__Customer__85FB4E3872752CA2").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Customer__Email").IsUnique();

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
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.TotalSpent)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.LastTierReviewedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<CustomerTierHistory>(entity =>
        {
            entity.HasKey(e => e.TierHistoryId);
            entity.ToTable("CustomerTierHistory");
            entity.Property(e => e.TierHistoryId).HasColumnName("TierHistoryID");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.PreviousTier).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.NewTier).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.QualifyingSpend).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ReviewType).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.ReviewedAt).HasColumnType("datetime");
            entity.HasIndex(e => new { e.CustomerId, e.ReviewedAt });

            entity.HasOne(e => e.Customer).WithMany(e => e.TierHistories)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_CustomerTierHistory_Customers");
        });

        modelBuilder.Entity<IncidentReport>(entity =>
        {
            entity.HasKey(e => e.ReportId);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ResolvedAt).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.Booking)
                .WithMany(p => p.IncidentReports)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Customer)
                .WithMany(p => p.IncidentReports)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ParkingReceipt>(entity =>
        {
            entity.HasKey(e => e.ReceiptId).HasName("PK__ParkingR__CC08C4009D92C04D");

            entity.HasIndex(e => e.BookingId, "UQ__ParkingR__73951ACC51E1CEE8").IsUnique();

            entity.Property(e => e.ReceiptId).HasColumnName("ReceiptID");
            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.IsCustomerLeaving).HasDefaultValue(false);
            entity.Property(e => e.IssueStaffId).HasColumnName("IssueStaffID");
            entity.Property(e => e.IssuedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Issued");
            entity.Property(e => e.VerifiedAt).HasColumnType("datetime");
            entity.Property(e => e.VerifyStaffId).HasColumnName("VerifyStaffID");

            entity.HasOne(d => d.Booking).WithOne(p => p.ParkingReceipt)
                .HasForeignKey<ParkingReceipt>(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ParkingRe__Booki__04E4BC85");

            entity.HasOne(d => d.IssueStaff).WithMany(p => p.ParkingReceiptIssueStaffs)
                .HasForeignKey(d => d.IssueStaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ParkingRe__Issue__05D8E0BE");

            entity.HasOne(d => d.VerifyStaff).WithMany(p => p.ParkingReceiptVerifyStaffs)
                .HasForeignKey(d => d.VerifyStaffId)
                .HasConstraintName("FK__ParkingRe__Verif__06CD04F7");
        });

        modelBuilder.Entity<PointLedger>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__PointLed__55433A4B69BABF45");

            entity.ToTable("PointLedger");

            entity.HasIndex(e => e.RewardRedemptionId, "UX_PointLedger_RewardRedemptionID")
                .IsUnique()
                .HasFilter("([RewardRedemptionID] IS NOT NULL)");

            entity.HasIndex(e => e.SourceTransactionId, "UX_PointLedger_ExpireSourceTransactionID")
                .IsUnique()
                .HasFilter("([SourceTransactionID] IS NOT NULL)");

            entity.HasIndex(
                e => new { e.TransactionType, e.ExpireDate },
                "IX_PointLedger_TransactionType_ExpireDate");

            entity.Property(e => e.TransactionId).HasColumnName("TransactionID");
            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ExpireDate).HasColumnType("datetime");
            entity.Property(e => e.RewardRedemptionId).HasColumnName("RewardRedemptionID");
            entity.Property(e => e.SourceTransactionId).HasColumnName("SourceTransactionID");
            entity.Property(e => e.TransactionType)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne<PointLedger>()
                .WithMany()
                .HasForeignKey(e => e.SourceTransactionId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_PointLedger_ExpireSourceTransaction");

            entity.HasOne(d => d.Booking).WithMany(p => p.PointLedgers)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK__PointLedg__Booki__68487DD7");

            entity.HasOne(d => d.Customer).WithMany(p => p.PointLedgers)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PointLedg__Custo__6754599E");

            entity.HasOne(d => d.RewardRedemption).WithOne(p => p.PointLedger)
                .HasForeignKey<PointLedger>(d => d.RewardRedemptionId)
                .HasConstraintName("FK_PointLedger_RewardRedemptions");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.PromotionId).HasName("PK__Promotio__52C42F2F7D471371");

            entity.HasIndex(e => e.PromoCode, "UQ__Promotio__32DBED350150C25C").IsUnique();

            entity.Property(e => e.PromotionId).HasColumnName("PromotionID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DiscountType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.DiscountValue).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MaxDiscount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PromoCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PromoName).HasMaxLength(100);
            entity.Property(e => e.PromoType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ServiceId).HasColumnName("ServiceID");
            entity.Property(e => e.TargetTier)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ValidFrom).HasColumnType("datetime");
            entity.Property(e => e.ValidTo).HasColumnType("datetime");

            entity.HasOne(d => d.Service).WithMany(p => p.Promotions)
                .HasForeignKey(d => d.ServiceId)
                .HasConstraintName("FK_Promotions_Services");
        });

        modelBuilder.Entity<Reward>(entity =>
        {
            entity.HasIndex(e => e.ServiceId, "IX_Rewards_ServiceID");

            entity.HasIndex(e => e.RewardName, "UQ_Rewards_RewardName").IsUnique();

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

            entity.HasOne(d => d.Service).WithMany(p => p.Rewards)
                .HasForeignKey(d => d.ServiceId)
                .HasConstraintName("FK_Rewards_Services");
        });

        modelBuilder.Entity<RewardRedemption>(entity =>
        {
            entity.HasKey(e => e.RedemptionId);

            entity.HasIndex(e => e.BookingId, "IX_RewardRedemptions_BookingID");

            entity.HasIndex(e => e.CustomerId, "IX_RewardRedemptions_CustomerID");

            entity.HasIndex(e => e.RewardId, "IX_RewardRedemptions_RewardID");

            entity.HasIndex(e => e.RequestId, "UQ_RewardRedemptions_RequestId").IsUnique();

            entity.Property(e => e.RedemptionId).HasColumnName("RedemptionID");
            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.RedeemedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RewardId).HasColumnName("RewardID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Issued");
            entity.Property(e => e.UsedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Booking).WithMany(p => p.RewardRedemptions)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK_RewardRedemptions_Bookings");

            entity.HasOne(d => d.Customer).WithMany(p => p.RewardRedemptions)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RewardRedemptions_Customers");

            entity.HasOne(d => d.Reward).WithMany(p => p.RewardRedemptions)
                .HasForeignKey(d => d.RewardId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RewardRedemptions_Rewards");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("PK__Services__C51BB0EA6F4EB55B");

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
            entity.HasKey(e => e.StaffId).HasName("PK__Staff__96D4AAF775894E90");

            entity.HasIndex(e => e.PhoneNumber, "UQ__Staff__85FB4E385B409A45").IsUnique();

            entity.Property(e => e.StaffId).HasColumnName("StaffID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Staff");
        });

        modelBuilder.Entity<TimeSlot>(entity =>
        {
            entity.HasKey(e => e.SlotId).HasName("PK__TimeSlot__0A124A4FD1897255");

            entity.Property(e => e.SlotId).HasColumnName("SlotID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            
            entity.Property(e => e.CarCapacity).HasDefaultValue(2);
            entity.Property(e => e.BikeCapacity).HasDefaultValue(5);
        });

        modelBuilder.Entity<TierRule>(entity =>
        {
            entity.HasKey(e => e.TierRuleId);
            entity.ToTable("TierRules");
            entity.Property(e => e.TierRuleId).HasColumnName("TierRuleID");
            entity.Property(e => e.TierName).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.MinimumSpend).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.QualificationMode)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasDefaultValue("OR");
            entity.Property(e => e.PointMultiplier).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.BenefitDescription).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.HasIndex(e => e.TierName).IsUnique();
            entity.HasIndex(e => e.Rank).IsUnique();

            entity.HasData(
                new TierRule
                {
                    TierRuleId = 1,
                    TierName = "Member",
                    Rank = 1,
                    MinimumSpend = 0,
                    MinimumVisits = 0,
                    QualificationMode = "OR",
                    EvaluationPeriodMonths = 12,
                    BookingWindowDays = 7,
                    PointMultiplier = 1.00m,
                    BenefitDescription = "Book up to 7 days in advance.",
                    IsActive = true,
                    UpdatedAt = new DateTime(2026, 8, 13, 0, 0, 0, DateTimeKind.Utc)
                },
                new TierRule
                {
                    TierRuleId = 2,
                    TierName = "Silver",
                    Rank = 2,
                    MinimumSpend = 500_000,
                    MinimumVisits = 5,
                    QualificationMode = "OR",
                    EvaluationPeriodMonths = 12,
                    BookingWindowDays = 10,
                    PointMultiplier = 1.10m,
                    BenefitDescription = "Book up to 10 days in advance and earn 10% bonus points.",
                    IsActive = true,
                    UpdatedAt = new DateTime(2026, 8, 13, 0, 0, 0, DateTimeKind.Utc)
                },
                new TierRule
                {
                    TierRuleId = 3,
                    TierName = "Gold",
                    Rank = 3,
                    MinimumSpend = 2_000_000,
                    MinimumVisits = 15,
                    QualificationMode = "OR",
                    EvaluationPeriodMonths = 12,
                    BookingWindowDays = 12,
                    PointMultiplier = 1.25m,
                    BenefitDescription = "Book up to 12 days in advance and earn 25% bonus points.",
                    IsActive = true,
                    UpdatedAt = new DateTime(2026, 8, 13, 0, 0, 0, DateTimeKind.Utc)
                },
                new TierRule
                {
                    TierRuleId = 4,
                    TierName = "Platinum",
                    Rank = 4,
                    MinimumSpend = 5_000_000,
                    MinimumVisits = 30,
                    QualificationMode = "OR",
                    EvaluationPeriodMonths = 12,
                    BookingWindowDays = 14,
                    PointMultiplier = 1.50m,
                    BenefitDescription = "Book up to 14 days in advance and earn 50% bonus points.",
                    IsActive = true,
                    UpdatedAt = new DateTime(2026, 8, 13, 0, 0, 0, DateTimeKind.Utc)
                });
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.VehicleId).HasName("PK__Vehicles__476B54B2CEE7BEC8");

            entity.HasIndex(e => e.LicensePlate, "UQ__Vehicles__026BC15C3EEB94D8").IsUnique();

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
                .HasConstraintName("FK__Vehicles__Custom__412EB0B6");
        });

        modelBuilder.Entity<SystemParameter>(entity =>
        {
            entity.ToTable("SystemParameters");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BikeDepositAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CarDepositPercentage).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.ContactPhone).HasMaxLength(20).IsUnicode(false);
            
            entity.HasData(
                new SystemParameter
                {
                    Id = 1,
                    BikeDepositAmount = 10000m,
                    CarDepositPercentage = 10m,
                    ContactPhone = "19001560",
                    CancellationRefundDays = 1
                });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
