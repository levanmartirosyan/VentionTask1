using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VentionTask1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CompositeIndexForSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sessions_UserId",
                table: "Sessions");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_UserId_IsRevoked",
                table: "Sessions",
                columns: new[] { "UserId", "IsRevoked" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sessions_UserId_IsRevoked",
                table: "Sessions");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_UserId",
                table: "Sessions",
                column: "UserId");
        }
    }
}
