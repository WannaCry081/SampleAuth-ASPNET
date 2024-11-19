using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sample_auth_aspnet.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTokenKeyProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Refresh",
                table: "Tokens",
                newName: "Key");

            migrationBuilder.RenameIndex(
                name: "IX_Tokens_Refresh",
                table: "Tokens",
                newName: "IX_Tokens_Key");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Key",
                table: "Tokens",
                newName: "Refresh");

            migrationBuilder.RenameIndex(
                name: "IX_Tokens_Key",
                table: "Tokens",
                newName: "IX_Tokens_Refresh");
        }
    }
}
