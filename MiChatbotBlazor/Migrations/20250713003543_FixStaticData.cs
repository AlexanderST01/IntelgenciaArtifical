using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiChatbotBlazor.Migrations
{
    /// <inheritdoc />
    public partial class FixStaticData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ChatMessages",
                keyColumn: "Id",
                keyValue: 1,
                column: "Timestamp",
                value: new DateTime(2025, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "ChatSessions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ChatMessages",
                keyColumn: "Id",
                keyValue: 1,
                column: "Timestamp",
                value: new DateTime(2025, 7, 13, 0, 19, 11, 863, DateTimeKind.Utc).AddTicks(5652));

            migrationBuilder.UpdateData(
                table: "ChatSessions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 13, 0, 19, 11, 863, DateTimeKind.Utc).AddTicks(2044), new DateTime(2025, 7, 13, 0, 19, 11, 863, DateTimeKind.Utc).AddTicks(1671) });
        }
    }
}
