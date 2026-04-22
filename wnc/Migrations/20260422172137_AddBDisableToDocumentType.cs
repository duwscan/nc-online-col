using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wnc.Migrations
{
    /// <inheritdoc />
    public partial class AddBDisableToDocumentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "bDisable",
                table: "document_types",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bDisable",
                table: "document_types");
        }
    }
}
