using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sample_auth_aspnet.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTokenProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "JTI",
                table: "Tokens",
                newName: "Refresh");

            migrationBuilder.RenameIndex(
                name: "IX_Tokens_JTI",
                table: "Tokens",
                newName: "IX_Tokens_Refresh");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Refresh",
                table: "Tokens",
                newName: "JTI");

            migrationBuilder.RenameIndex(
                name: "IX_Tokens_Refresh",
                table: "Tokens",
                newName: "IX_Tokens_JTI");
        }
    }
}
