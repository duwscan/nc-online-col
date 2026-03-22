using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wnc.Migrations
{
    /// <inheritdoc />
    public partial class AllowZeroTrainingProgramQuota : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_training_programs_quota",
                table: "training_programs");

            migrationBuilder.AddCheckConstraint(
                name: "ck_training_programs_quota",
                table: "training_programs",
                sql: "[Quota] >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_training_programs_quota",
                table: "training_programs");

            migrationBuilder.AddCheckConstraint(
                name: "ck_training_programs_quota",
                table: "training_programs",
                sql: "[Quota] > 0");
        }
    }
}
