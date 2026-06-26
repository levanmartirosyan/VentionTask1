using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VentionTask1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedSampleDataSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Organizations",
                columns: new[] { "Id", "CreatedAt", "Name" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Vention" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "OrganizationId", "PasswordHash", "Username" },
                values: new object[] { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@example.com", new Guid("11111111-1111-1111-1111-111111111111"), "Admin123!", "Admin" });

            migrationBuilder.InsertData(
                table: "Sessions",
                columns: new[] { "Id", "CreatedAt", "ExpiresAt", "IsRevoked", "RefreshToken", "UserId" },
                values: new object[] { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "test-refresh-token", new Guid("22222222-2222-2222-2222-222222222222") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Sessions",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "Organizations",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));
        }
    }
}
