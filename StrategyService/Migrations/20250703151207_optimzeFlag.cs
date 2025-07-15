using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BN.PROJECT.StrategyService.Migrations
{
    /// <inheritdoc />
    public partial class optimzeFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Optimized",
                table: "Strategies",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Optimized",
                table: "Strategies");
        }
    }
}
