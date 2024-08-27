using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BN.TRADER.AlpacaService.Migrations
{
    /// <inheritdoc />
    public partial class asset_model_change : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSelected",
                table: "Assets",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSelected",
                table: "Assets");
        }
    }
}