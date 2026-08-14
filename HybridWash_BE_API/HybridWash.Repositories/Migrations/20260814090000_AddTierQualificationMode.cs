using HybridWash.Repositories.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HybridWash.Repositories.Migrations;

[DbContext(typeof(AutowashContext))]
[Migration("20260814090000_AddTierQualificationMode")]
public partial class AddTierQualificationMode : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "QualificationMode",
            table: "TierRules",
            type: "varchar(3)",
            unicode: false,
            maxLength: 3,
            nullable: false,
            defaultValue: "OR");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "QualificationMode",
            table: "TierRules");
    }
}
