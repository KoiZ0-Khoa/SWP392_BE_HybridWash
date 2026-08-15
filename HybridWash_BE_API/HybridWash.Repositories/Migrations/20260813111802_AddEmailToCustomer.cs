using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HybridWash.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Customers",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE [Customers] SET [Email] = CONCAT('customer', [CustomerId], '@hybridwash.com') WHERE [Email] = '' OR [Email] IS NULL;");

            migrationBuilder.CreateIndex(
                name: "UQ__Customer__Email",
                table: "Customers",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ__Customer__Email",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Customers");
        }
    }
}
