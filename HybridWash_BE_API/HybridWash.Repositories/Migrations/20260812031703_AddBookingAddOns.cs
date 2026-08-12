using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HybridWash.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingAddOns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookingAddOns",
                columns: table => new
                {
                    BookingAddOnID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    PromotionID = table.Column<int>(type: "int", nullable: true),
                    RedemptionID = table.Column<int>(type: "int", nullable: true),
                    OriginalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "Pending"),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingAddOns", x => x.BookingAddOnID);
                    table.ForeignKey(
                        name: "FK_BookingAddOns_Bookings",
                        column: x => x.BookingID,
                        principalTable: "Bookings",
                        principalColumn: "BookingID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingAddOns_Promotions",
                        column: x => x.PromotionID,
                        principalTable: "Promotions",
                        principalColumn: "PromotionID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookingAddOns_RewardRedemptions",
                        column: x => x.RedemptionID,
                        principalTable: "RewardRedemptions",
                        principalColumn: "RedemptionID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookingAddOns_Services",
                        column: x => x.ServiceID,
                        principalTable: "Services",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookingAddOns_BookingID",
                table: "BookingAddOns",
                column: "BookingID");

            migrationBuilder.CreateIndex(
                name: "IX_BookingAddOns_PromotionID",
                table: "BookingAddOns",
                column: "PromotionID");

            migrationBuilder.CreateIndex(
                name: "IX_BookingAddOns_ServiceID",
                table: "BookingAddOns",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "UX_BookingAddOns_RedemptionID",
                table: "BookingAddOns",
                column: "RedemptionID",
                unique: true,
                filter: "[RedemptionID] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingAddOns");
        }
    }
}
