using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace wnc.Migrations
{
    /// <inheritdoc />
    public partial class SeedMoreRounds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "admission_rounds",
                columns: new[] { "Id", "AdmissionYear", "AllowEnrollmentConfirmation", "CreatedAt", "CreatedBy", "EndAt", "Notes", "RoundCode", "RoundName", "StartAt", "Status", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("aaaaaaa1-1111-1111-1111-111111111111"), 2026, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 6, 30, 23, 59, 59, 0, DateTimeKind.Utc), "Dot xet tuyen chinh quy nam 2026", "DOT1-2026", "Dot 1 nam 2026", new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), "PUBLISHED", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("bbbbbbb1-1111-1111-1111-111111111111"), 2025, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 6, 30, 23, 59, 59, 0, DateTimeKind.Utc), "Dot xet tuyen chinh quy nam 2025", "DOT1-2025", "Dot 1 nam 2025", new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), "PUBLISHED", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("bbbbbbb2-1111-1111-1111-111111111111"), 2026, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 9, 30, 23, 59, 59, 0, DateTimeKind.Utc), "Dot xet tuyen chinh quy dot 2 nam 2026", "DOT2-2026", "Dot 2 nam 2026", new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), "PUBLISHED", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("bbbbbbb3-1111-1111-1111-111111111111"), 2027, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2027, 6, 30, 23, 59, 59, 0, DateTimeKind.Utc), "Dot xet tuyen chinh quy nam 2027", "DOT3-2027", "Dot 3 nam 2027", new DateTime(2027, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), "PUBLISHED", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "majors",
                columns: new[] { "Id", "CreatedAt", "Description", "DisplayOrder", "MajorCode", "MajorName", "ProgramId", "Quota", "Status", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("aaaaaaa2-1111-1111-1111-111111111111"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cong nghe thong tin", 1, "CNTT", "Cong nghe thong tin", new Guid("99999999-7777-7777-7777-777777777777"), 150, "ACTIVE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("aaaaaaa3-1111-1111-1111-111111111111"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ke toan doanh nghiep", 2, "KT", "Ke toan", new Guid("99999999-7777-7777-7777-777777777777"), 100, "ACTIVE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("aaaaaaa4-1111-1111-1111-111111111111"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Marketing so", 3, "MK", "Marketing", new Guid("99999999-7777-7777-7777-777777777777"), 80, "ACTIVE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "round_programs",
                columns: new[] { "Id", "CreatedAt", "MajorId", "ProgramId", "PublishedQuota", "Quota", "RoundId", "Status", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("aaaaaaa5-1111-1111-1111-111111111111"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("aaaaaaa2-1111-1111-1111-111111111111"), new Guid("99999999-7777-7777-7777-777777777777"), 120, 150, new Guid("aaaaaaa1-1111-1111-1111-111111111111"), "ACTIVE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("aaaaaaa6-1111-1111-1111-111111111111"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("aaaaaaa3-1111-1111-1111-111111111111"), new Guid("99999999-7777-7777-7777-777777777777"), 80, 100, new Guid("aaaaaaa1-1111-1111-1111-111111111111"), "ACTIVE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("aaaaaaa7-1111-1111-1111-111111111111"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("aaaaaaa4-1111-1111-1111-111111111111"), new Guid("99999999-7777-7777-7777-777777777777"), 60, 80, new Guid("aaaaaaa1-1111-1111-1111-111111111111"), "ACTIVE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("bbbbbb01-1111-1111-1111-111111111111"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("aaaaaaa2-1111-1111-1111-111111111111"), new Guid("99999999-7777-7777-7777-777777777777"), 90, 100, new Guid("bbbbbbb1-1111-1111-1111-111111111111"), "ACTIVE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("bbbbbb02-1111-1111-1111-111111111111"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("aaaaaaa3-1111-1111-1111-111111111111"), new Guid("99999999-7777-7777-7777-777777777777"), 70, 80, new Guid("bbbbbbb1-1111-1111-1111-111111111111"), "ACTIVE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("bbbbbb03-1111-1111-1111-111111111111"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("aaaaaaa2-1111-1111-1111-111111111111"), new Guid("99999999-7777-7777-7777-777777777777"), 80, 100, new Guid("bbbbbbb2-1111-1111-1111-111111111111"), "ACTIVE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("bbbbbb04-1111-1111-1111-111111111111"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("aaaaaaa2-1111-1111-1111-111111111111"), new Guid("99999999-7777-7777-7777-777777777777"), 100, 120, new Guid("bbbbbbb3-1111-1111-1111-111111111111"), "ACTIVE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "round_admission_methods",
                columns: new[] { "Id", "CalculationRule", "CombinationCode", "CreatedAt", "MethodId", "MinimumScore", "PriorityPolicy", "RoundProgramId", "Status" },
                values: new object[,]
                {
                    { new Guid("aaaaaa01-1111-1111-1111-111111111111"), null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("aaaaaaaa-1111-1111-1111-111111111111"), 7.0m, null, new Guid("aaaaaaa5-1111-1111-1111-111111111111"), "ACTIVE" },
                    { new Guid("aaaaaa02-1111-1111-1111-111111111111"), null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("bbbbbbbb-2222-2222-2222-222222222222"), 18.0m, null, new Guid("aaaaaaa5-1111-1111-1111-111111111111"), "ACTIVE" },
                    { new Guid("aaaaaa03-1111-1111-1111-111111111111"), null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("aaaaaaaa-1111-1111-1111-111111111111"), 6.5m, null, new Guid("aaaaaaa6-1111-1111-1111-111111111111"), "ACTIVE" },
                    { new Guid("aaaaaa04-1111-1111-1111-111111111111"), null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("aaaaaaaa-1111-1111-1111-111111111111"), 6.0m, null, new Guid("aaaaaaa7-1111-1111-1111-111111111111"), "ACTIVE" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "round_admission_methods",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaa01-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "round_admission_methods",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaa02-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "round_admission_methods",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaa03-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "round_admission_methods",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaa04-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "round_programs",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbb01-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "round_programs",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbb02-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "round_programs",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbb03-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "round_programs",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbb04-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "admission_rounds",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbb1-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "admission_rounds",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbb2-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "admission_rounds",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbb3-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "round_programs",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaa5-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "round_programs",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaa6-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "round_programs",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaa7-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "admission_rounds",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaa1-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "majors",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaa2-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "majors",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaa3-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "majors",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaa4-1111-1111-1111-111111111111"));
        }
    }
}
