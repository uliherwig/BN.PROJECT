using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BN.PROJECT.StrategyService.Migrations
{
    /// <inheritdoc />
    public partial class SettingsChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TestStamp",
                table: "Strategies",
                newName: "StampStart");

            migrationBuilder.AddColumn<DateTime>(
                name: "StampEnd",
                table: "Strategies",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StampEnd",
                table: "Strategies");

            migrationBuilder.RenameColumn(
                name: "StampStart",
                table: "Strategies",
                newName: "TestStamp");
        }
    }
}
