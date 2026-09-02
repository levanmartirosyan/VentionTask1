using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VentionTask1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLoginSessionTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sessions_RefreshToken",
                table: "Sessions");

            migrationBuilder.DeleteData(
                table: "Sessions",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "Sessions");

            migrationBuilder.RenameColumn(
                name: "IsRevoked",
                table: "Sessions",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "ExpiresAt",
                table: "Sessions",
                newName: "LoggedInAt");

            migrationBuilder.RenameIndex(
                name: "IX_Sessions_UserId_IsRevoked",
                table: "Sessions",
                newName: "IX_Sessions_UserId_IsActive");

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "Sessions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LoggedOutAt",
                table: "Sessions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "Sessions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "LoggedOutAt",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                table: "Sessions");

            migrationBuilder.RenameColumn(
                name: "LoggedInAt",
                table: "Sessions",
                newName: "ExpiresAt");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Sessions",
                newName: "IsRevoked");

            migrationBuilder.RenameIndex(
                name: "IX_Sessions_UserId_IsActive",
                table: "Sessions",
                newName: "IX_Sessions_UserId_IsRevoked");

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "Sessions",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "Sessions",
                columns: new[] { "Id", "CreatedAt", "ExpiresAt", "IsRevoked", "RefreshToken", "UpdatedAt", "UserId" },
                values: new object[] { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "test-refresh-token", null, new Guid("22222222-2222-2222-2222-222222222222") });

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_RefreshToken",
                table: "Sessions",
                column: "RefreshToken",
                unique: true);
        }
    }
}
