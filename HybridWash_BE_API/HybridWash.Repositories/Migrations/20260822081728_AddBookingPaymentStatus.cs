using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HybridWash.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingPaymentStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "Bookings",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                defaultValue: "Unpaid");

            migrationBuilder.Sql(@"
                UPDATE [Bookings]
                SET [PaymentStatus] = CASE
                    WHEN [Status] IN ('Completed', 'CheckedOut') THEN 'Paid'
                    WHEN [Status] IN ('Deposited', 'Confirmed', 'Washing', 'Cancelled', 'RefundPending', 'NoShow')
                         AND COALESCE([FinalPrice], [OriginalPrice], 0) <= COALESCE([DepositAmount], 0)
                        THEN 'Paid'
                    WHEN [Status] IN ('Deposited', 'Confirmed', 'Washing', 'Cancelled', 'RefundPending', 'NoShow')
                         AND COALESCE([DepositAmount], 0) > 0
                        THEN 'PartiallyPaid'
                    ELSE 'Unpaid'
                END;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "Bookings");
        }
    }
}
