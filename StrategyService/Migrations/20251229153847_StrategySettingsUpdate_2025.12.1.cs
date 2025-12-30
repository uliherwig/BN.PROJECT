using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BN.PROJECT.StrategyService.Migrations
{
    /// <inheritdoc />
    public partial class StrategySettingsUpdate_2025121 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "OvernightFeeRate",
                table: "Strategies",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "ReverseTrade",
                table: "Strategies",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "SpreadPerTrade",
                table: "Strategies",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OvernightFeeRate",
                table: "Strategies");

            migrationBuilder.DropColumn(
                name: "ReverseTrade",
                table: "Strategies");

            migrationBuilder.DropColumn(
                name: "SpreadPerTrade",
                table: "Strategies");
        }
    }
}
