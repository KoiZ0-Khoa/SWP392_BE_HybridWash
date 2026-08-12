using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HybridWash.Repositories.Migrations
{
    public partial class UpdateTimeSlotCapacity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Capacity",
                table: "TimeSlots");

            migrationBuilder.AddColumn<int>(
                name: "BikeCapacity",
                table: "TimeSlots",
                type: "int",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.AddColumn<int>(
                name: "CarCapacity",
                table: "TimeSlots",
                type: "int",
                nullable: false,
                defaultValue: 2);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BikeCapacity",
                table: "TimeSlots");

            migrationBuilder.DropColumn(
                name: "CarCapacity",
                table: "TimeSlots");

            migrationBuilder.AddColumn<int>(
                name: "Capacity",
                table: "TimeSlots",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
