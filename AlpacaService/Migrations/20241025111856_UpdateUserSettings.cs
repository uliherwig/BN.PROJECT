using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BN.PROJECT.AlpacaService.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserSettings",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "UserSettings");

            migrationBuilder.RenameColumn(
                name: "Symbols",
                table: "UserSettings",
                newName: "UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserSettings",
                table: "UserSettings",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserSettings",
                table: "UserSettings");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UserSettings",
                newName: "Symbols");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "UserSettings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserSettings",
                table: "UserSettings",
                column: "Email");
        }
    }
}