using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wnc.Migrations
{
    /// <inheritdoc />
    public partial class AddDeletedAtToAdmissionRound : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "admission_rounds",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "admission_rounds");
        }
    }
}
