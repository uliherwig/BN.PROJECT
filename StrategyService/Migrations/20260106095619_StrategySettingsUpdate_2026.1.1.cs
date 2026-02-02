using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BN.PROJECT.StrategyService.Migrations
{
    /// <inheritdoc />
    public partial class StrategySettingsUpdate_202611 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StrategyType",
                table: "Positions");

            migrationBuilder.AddColumn<decimal>(
                name: "OvernightFeeRate",
                table: "Positions",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SpreadPerTrade",
                table: "Positions",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OvernightFeeRate",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "SpreadPerTrade",
                table: "Positions");

            migrationBuilder.AddColumn<int>(
                name: "StrategyType",
                table: "Positions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
