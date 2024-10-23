using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BN.PROJECT.StrategyService.Migrations
{
    /// <inheritdoc />
    public partial class ChangePositionAndSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PrevHigh",
                table: "Positions",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrevLow",
                table: "Positions",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Broker",
                table: "BacktestSettings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "TrailingStop",
                table: "BacktestSettings",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateIndex(
                name: "IX_BacktestSettings_Name",
                table: "BacktestSettings",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BacktestSettings_Name",
                table: "BacktestSettings");

            migrationBuilder.DropColumn(
                name: "PrevHigh",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "PrevLow",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "Broker",
                table: "BacktestSettings");

            migrationBuilder.DropColumn(
                name: "TrailingStop",
                table: "BacktestSettings");
        }
    }
}
