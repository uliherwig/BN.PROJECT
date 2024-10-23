using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BN.PROJECT.StrategyService.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSettings2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TakeProfitFactor",
                table: "BacktestSettings",
                newName: "TakeProfitPercent");

            migrationBuilder.RenameColumn(
                name: "StopLossFactor",
                table: "BacktestSettings",
                newName: "StopLossPercent");

            migrationBuilder.AddColumn<bool>(
                name: "Bookmarked",
                table: "BacktestSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "StopLossStrategy",
                table: "BacktestSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bookmarked",
                table: "BacktestSettings");

            migrationBuilder.DropColumn(
                name: "StopLossStrategy",
                table: "BacktestSettings");

            migrationBuilder.RenameColumn(
                name: "TakeProfitPercent",
                table: "BacktestSettings",
                newName: "TakeProfitFactor");

            migrationBuilder.RenameColumn(
                name: "StopLossPercent",
                table: "BacktestSettings",
                newName: "StopLossFactor");
        }
    }
}
