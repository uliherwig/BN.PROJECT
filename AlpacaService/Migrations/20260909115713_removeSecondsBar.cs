using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BN.PROJECT.AlpacaService.Migrations
{
    /// <inheritdoc />
    public partial class removeSecondsBar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BarsPerSeconds");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BarsPerSeconds",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    C = table.Column<decimal>(type: "numeric", nullable: false),
                    H = table.Column<decimal>(type: "numeric", nullable: false),
                    L = table.Column<decimal>(type: "numeric", nullable: false),
                    N = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    O = table.Column<decimal>(type: "numeric", nullable: false),
                    Symbol = table.Column<string>(type: "text", nullable: false),
                    T = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    V = table.Column<decimal>(type: "numeric", nullable: false),
                    Vw = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BarsPerSeconds", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BarsPerSeconds_Symbol_T",
                table: "BarsPerSeconds",
                columns: new[] { "Symbol", "T" },
                unique: true);
        }
    }
}
