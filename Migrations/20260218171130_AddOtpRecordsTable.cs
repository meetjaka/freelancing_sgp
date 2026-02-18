using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGP_Freelancing.Migrations
{
    /// <inheritdoc />
    public partial class AddOtpRecordsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OtpRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Otp = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpRecords", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 18, 17, 11, 29, 328, DateTimeKind.Utc).AddTicks(2226));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 18, 17, 11, 29, 328, DateTimeKind.Utc).AddTicks(2230));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 18, 17, 11, 29, 328, DateTimeKind.Utc).AddTicks(2231));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 18, 17, 11, 29, 328, DateTimeKind.Utc).AddTicks(2234));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 18, 17, 11, 29, 328, DateTimeKind.Utc).AddTicks(2236));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 18, 17, 11, 29, 328, DateTimeKind.Utc).AddTicks(2237));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 18, 17, 11, 29, 328, DateTimeKind.Utc).AddTicks(2464));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 18, 17, 11, 29, 328, DateTimeKind.Utc).AddTicks(2466));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 18, 17, 11, 29, 328, DateTimeKind.Utc).AddTicks(2468));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 18, 17, 11, 29, 328, DateTimeKind.Utc).AddTicks(2469));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 18, 17, 11, 29, 328, DateTimeKind.Utc).AddTicks(2471));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 18, 17, 11, 29, 328, DateTimeKind.Utc).AddTicks(2472));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 18, 17, 11, 29, 328, DateTimeKind.Utc).AddTicks(2474));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 18, 17, 11, 29, 328, DateTimeKind.Utc).AddTicks(2476));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 18, 17, 11, 29, 328, DateTimeKind.Utc).AddTicks(2482));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 18, 17, 11, 29, 328, DateTimeKind.Utc).AddTicks(2484));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 18, 17, 11, 29, 328, DateTimeKind.Utc).AddTicks(2485));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 18, 17, 11, 29, 328, DateTimeKind.Utc).AddTicks(2487));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 18, 17, 11, 29, 328, DateTimeKind.Utc).AddTicks(2536));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 18, 17, 11, 29, 328, DateTimeKind.Utc).AddTicks(2538));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 18, 17, 11, 29, 328, DateTimeKind.Utc).AddTicks(2539));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 18, 17, 11, 29, 328, DateTimeKind.Utc).AddTicks(2541));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 18, 17, 11, 29, 328, DateTimeKind.Utc).AddTicks(2543));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 18, 17, 11, 29, 328, DateTimeKind.Utc).AddTicks(2544));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 18, 17, 11, 29, 328, DateTimeKind.Utc).AddTicks(2546));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 18, 17, 11, 29, 328, DateTimeKind.Utc).AddTicks(2548));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OtpRecords");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 9, 5, 58, 9, 881, DateTimeKind.Utc).AddTicks(6402));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 9, 5, 58, 9, 881, DateTimeKind.Utc).AddTicks(6405));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 9, 5, 58, 9, 881, DateTimeKind.Utc).AddTicks(6407));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 9, 5, 58, 9, 881, DateTimeKind.Utc).AddTicks(6409));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 9, 5, 58, 9, 881, DateTimeKind.Utc).AddTicks(6410));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 9, 5, 58, 9, 881, DateTimeKind.Utc).AddTicks(6412));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 9, 5, 58, 9, 881, DateTimeKind.Utc).AddTicks(6614));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 9, 5, 58, 9, 881, DateTimeKind.Utc).AddTicks(6616));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 9, 5, 58, 9, 881, DateTimeKind.Utc).AddTicks(6618));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 9, 5, 58, 9, 881, DateTimeKind.Utc).AddTicks(6620));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 9, 5, 58, 9, 881, DateTimeKind.Utc).AddTicks(6621));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 9, 5, 58, 9, 881, DateTimeKind.Utc).AddTicks(6623));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 9, 5, 58, 9, 881, DateTimeKind.Utc).AddTicks(6625));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 9, 5, 58, 9, 881, DateTimeKind.Utc).AddTicks(6627));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 9, 5, 58, 9, 881, DateTimeKind.Utc).AddTicks(6628));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 9, 5, 58, 9, 881, DateTimeKind.Utc).AddTicks(6630));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 9, 5, 58, 9, 881, DateTimeKind.Utc).AddTicks(6632));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 9, 5, 58, 9, 881, DateTimeKind.Utc).AddTicks(6633));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 9, 5, 58, 9, 881, DateTimeKind.Utc).AddTicks(6635));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 9, 5, 58, 9, 881, DateTimeKind.Utc).AddTicks(6637));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 9, 5, 58, 9, 881, DateTimeKind.Utc).AddTicks(6638));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 9, 5, 58, 9, 881, DateTimeKind.Utc).AddTicks(6640));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 9, 5, 58, 9, 881, DateTimeKind.Utc).AddTicks(6642));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 9, 5, 58, 9, 881, DateTimeKind.Utc).AddTicks(6643));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 9, 5, 58, 9, 881, DateTimeKind.Utc).AddTicks(6645));

            migrationBuilder.UpdateData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 9, 5, 58, 9, 881, DateTimeKind.Utc).AddTicks(6647));
        }
    }
}
