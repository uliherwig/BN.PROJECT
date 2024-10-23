using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BN.PROJECT.AlpacaService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
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
                    Symbol = table.Column<string>(type: "text", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientOrderId = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubmittedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FilledAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReplacedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Symbol = table.Column<string>(type: "text", nullable: false),
                    Notional = table.Column<decimal>(type: "numeric", nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric", nullable: true),
                    FilledQuantity = table.Column<decimal>(type: "numeric", nullable: false),
                    IntegerQuantity = table.Column<long>(type: "bigint", nullable: false),
                    IntegerFilledQuantity = table.Column<long>(type: "bigint", nullable: false),
                    OrderType = table.Column<int>(type: "integer", nullable: false),
                    OrderClass = table.Column<int>(type: "integer", nullable: false),
                    OrderSide = table.Column<int>(type: "integer", nullable: false),
                    TimeInForce = table.Column<int>(type: "integer", nullable: false),
                    LimitPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    StopPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    TrailOffsetInDollars = table.Column<decimal>(type: "numeric", nullable: true),
                    TrailOffsetInPercent = table.Column<decimal>(type: "numeric", nullable: true),
                    HighWaterMark = table.Column<decimal>(type: "numeric", nullable: true),
                    AverageFillPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    OrderStatus = table.Column<int>(type: "integer", nullable: false),
                    ReplacedByOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReplacesOrderId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.OrderId);
                });

            migrationBuilder.CreateTable(
                name: "Positions",
                columns: table => new
                {
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Symbol = table.Column<string>(type: "text", nullable: false),
                    Exchange = table.Column<int>(type: "integer", nullable: false),
                    AssetClass = table.Column<int>(type: "integer", nullable: false),
                    AverageEntryPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    IntegerQuantity = table.Column<long>(type: "bigint", nullable: false),
                    AvailableQuantity = table.Column<decimal>(type: "numeric", nullable: false),
                    IntegerAvailableQuantity = table.Column<long>(type: "bigint", nullable: false),
                    Side = table.Column<int>(type: "integer", nullable: false),
                    MarketValue = table.Column<decimal>(type: "numeric", nullable: true),
                    CostBasis = table.Column<decimal>(type: "numeric", nullable: false),
                    UnrealizedProfitLoss = table.Column<decimal>(type: "numeric", nullable: true),
                    UnrealizedProfitLossPercent = table.Column<decimal>(type: "numeric", nullable: true),
                    IntradayUnrealizedProfitLoss = table.Column<decimal>(type: "numeric", nullable: true),
                    IntradayUnrealizedProfitLossPercent = table.Column<decimal>(type: "numeric", nullable: true),
                    AssetCurrentPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    AssetLastPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    AssetChangePercent = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.AssetId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assets");

            migrationBuilder.DropTable(
                name: "Bars");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Positions");
        }
    }
}
