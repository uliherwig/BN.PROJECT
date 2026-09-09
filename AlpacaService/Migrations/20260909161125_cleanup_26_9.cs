using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BN.PROJECT.AlpacaService.Migrations
{
    /// <inheritdoc />
    public partial class cleanup_26_9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Executions");

            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Bars_Symbol_T\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Executions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Assets = table.Column<string>(type: "text", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StrategyId = table.Column<Guid>(type: "uuid", nullable: false),
                    StrategyType = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Executions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bars_Symbol_T",
                table: "Bars",
                columns: new[] { "Symbol", "T" },
                unique: true);
        }
    }
}
