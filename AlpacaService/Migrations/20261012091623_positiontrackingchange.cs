using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BN.PROJECT.AlpacaService.Migrations
{
    /// <inheritdoc />
    public partial class positiontrackingchange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Symbol",
                table: "PositionTrackings",
                newName: "Asset");

            migrationBuilder.AddColumn<decimal>(
                name: "ClosePrice",
                table: "PositionTrackings",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OpenPrice",
                table: "PositionTrackings",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "PositionTrackings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Side",
                table: "PositionTrackings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "StrategyId",
                table: "PositionTrackings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClosePrice",
                table: "PositionTrackings");

            migrationBuilder.DropColumn(
                name: "OpenPrice",
                table: "PositionTrackings");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "PositionTrackings");

            migrationBuilder.DropColumn(
                name: "Side",
                table: "PositionTrackings");

            migrationBuilder.DropColumn(
                name: "StrategyId",
                table: "PositionTrackings");

            migrationBuilder.RenameColumn(
                name: "Asset",
                table: "PositionTrackings",
                newName: "Symbol");
        }
    }
}
