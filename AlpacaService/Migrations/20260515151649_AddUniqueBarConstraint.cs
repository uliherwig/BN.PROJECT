using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BN.PROJECT.AlpacaService.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueBarConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Bars_Symbol_T",
                table: "Bars",
                columns: new[] { "Symbol", "T" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bars_Symbol_T",
                table: "Bars");
        }
    }
}
