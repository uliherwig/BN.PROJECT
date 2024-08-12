using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BN.TRADER.AlpacaService.Migrations
{
    /// <inheritdoc />
    public partial class initial_create : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Symbol = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.AssetId);
                });

            migrationBuilder.CreateTable(
                name: "Bars",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    C = table.Column<decimal>(type: "numeric", nullable: false),
                    H = table.Column<decimal>(type: "numeric", nullable: false),
                    L = table.Column<decimal>(type: "numeric", nullable: false),
                    N = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    O = table.Column<decimal>(type: "numeric", nullable: false),
                    T = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    V = table.Column<decimal>(type: "numeric", nullable: false),
                    Vw = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bars", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assets");

            migrationBuilder.DropTable(
                name: "Bars");
        }
    }
}
