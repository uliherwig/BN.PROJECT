using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BN.TRADER.AlpacaService.Migrations
{
    /// <inheritdoc />
    public partial class bars_symbol_added : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Symbol",
                table: "Bars",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Symbol",
                table: "Bars");
        }
    }
}
