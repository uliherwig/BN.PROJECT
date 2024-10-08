using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BN.PROJECT.StrategyService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BacktestSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Symbol = table.Column<string>(type: "text", nullable: false),
                    TakeProfitFactor = table.Column<double>(type: "double precision", nullable: false),
                    StopLossFactor = table.Column<double>(type: "double precision", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Strategy = table.Column<int>(type: "integer", nullable: false),
                    TimeFrame = table.Column<int>(type: "integer", nullable: false),
                    AllowOvernight = table.Column<bool>(type: "boolean", nullable: false),
                    UserEmail = table.Column<string>(type: "text", nullable: false),
                    TestStamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BacktestSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Positions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Symbol = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Side = table.Column<int>(type: "integer", nullable: false),
                    PriceOpen = table.Column<decimal>(type: "numeric", nullable: false),
                    PriceClose = table.Column<decimal>(type: "numeric", nullable: false),
                    ProfitLoss = table.Column<decimal>(type: "numeric", nullable: false),
                    TakeProfit = table.Column<decimal>(type: "numeric", nullable: false),
                    StopLoss = table.Column<decimal>(type: "numeric", nullable: false),
                    StampOpened = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StampClosed = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CloseSignal = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BacktestSettings");

            migrationBuilder.DropTable(
                name: "Positions");
        }
    }
}
