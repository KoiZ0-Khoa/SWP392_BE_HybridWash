using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HybridWash.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddPointExpiryProcessing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SourceTransactionID",
                table: "PointLedger",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PointLedger_TransactionType_ExpireDate",
                table: "PointLedger",
                columns: new[] { "TransactionType", "ExpireDate" });

            migrationBuilder.CreateIndex(
                name: "UX_PointLedger_ExpireSourceTransactionID",
                table: "PointLedger",
                column: "SourceTransactionID",
                unique: true,
                filter: "([SourceTransactionID] IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_PointLedger_ExpireSourceTransaction",
                table: "PointLedger",
                column: "SourceTransactionID",
                principalTable: "PointLedger",
                principalColumn: "TransactionID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PointLedger_ExpireSourceTransaction",
                table: "PointLedger");

            migrationBuilder.DropIndex(
                name: "IX_PointLedger_TransactionType_ExpireDate",
                table: "PointLedger");

            migrationBuilder.DropIndex(
                name: "UX_PointLedger_ExpireSourceTransactionID",
                table: "PointLedger");

            migrationBuilder.DropColumn(
                name: "SourceTransactionID",
                table: "PointLedger");
        }
    }
}
