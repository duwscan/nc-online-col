using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace wnc.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "admission_methods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MethodCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MethodName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admission_methods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "document_types",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DocumentName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_types", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "notification_templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TemplateName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Channel = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SubjectTemplate = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    BodyTemplate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSystemRole = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "training_programs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProgramCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProgramName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    EducationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TuitionFee = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    DurationText = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Quota = table.Column<int>(type: "int", nullable: false),
                    ManagingUnit = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_programs", x => x.Id);
                    table.CheckConstraint("ck_training_programs_quota", "[Quota] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EmailVerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PhoneVerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "majors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProgramId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MajorCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MajorName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quota = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_majors", x => x.Id);
                    table.CheckConstraint("ck_majors_quota", "[Quota] >= 0");
                    table.ForeignKey(
                        name: "FK_majors_training_programs_ProgramId",
                        column: x => x.ProgramId,
                        principalTable: "training_programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "admission_rounds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoundCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RoundName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    AdmissionYear = table.Column<int>(type: "int", nullable: false),
                    StartAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AllowEnrollmentConfirmation = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admission_rounds", x => x.Id);
                    table.CheckConstraint("ck_admission_rounds_time", "[StartAt] < [EndAt]");
                    table.ForeignKey(
                        name: "FK_admission_rounds_users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OldData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_audit_logs_users_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "auth_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LoginIdentifier = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FailureReason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LoggedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auth_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_auth_logs_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "candidates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NationalId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AddressLine = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Ward = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    District = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProvinceCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AvatarFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_candidates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_candidates_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "password_reset_tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TokenType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ExpiredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_password_reset_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_password_reset_tokens_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "system_configs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConfigKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ConfigValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_configs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_system_configs_users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_roles_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_roles_users_AssignedBy",
                        column: x => x.AssignedBy,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_roles_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "round_programs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoundId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProgramId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MajorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Quota = table.Column<int>(type: "int", nullable: false),
                    PublishedQuota = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_round_programs", x => x.Id);
                    table.CheckConstraint("ck_round_programs_published_quota", "[PublishedQuota] IS NULL OR [PublishedQuota] >= 0");
                    table.CheckConstraint("ck_round_programs_quota", "[Quota] >= 0");
                    table.ForeignKey(
                        name: "FK_round_programs_admission_rounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "admission_rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_round_programs_majors_MajorId",
                        column: x => x.MajorId,
                        principalTable: "majors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_round_programs_training_programs_ProgramId",
                        column: x => x.ProgramId,
                        principalTable: "training_programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "candidate_education_profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchoolName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    EducationLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GraduationYear = table.Column<int>(type: "int", nullable: true),
                    Gpa = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    AcademicRank = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ProvinceCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_candidate_education_profiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_candidate_education_profiles_candidates_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "candidates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "candidate_priority_profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CandidateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PriorityType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PriorityCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ScoreValue = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    EvidenceFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_candidate_priority_profiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_candidate_priority_profiles_candidates_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "candidates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "admission_applications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CandidateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoundProgramId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SubmissionNumber = table.Column<int>(type: "int", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastResubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewDeadlineAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admission_applications", x => x.Id);
                    table.CheckConstraint("ck_admission_applications_submission_number", "[SubmissionNumber] >= 0");
                    table.ForeignKey(
                        name: "FK_admission_applications_candidates_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "candidates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_admission_applications_round_programs_RoundProgramId",
                        column: x => x.RoundProgramId,
                        principalTable: "round_programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "round_admission_methods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoundProgramId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MethodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CombinationCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MinimumScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    PriorityPolicy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CalculationRule = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_round_admission_methods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_round_admission_methods_admission_methods_MethodId",
                        column: x => x.MethodId,
                        principalTable: "admission_methods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_round_admission_methods_round_programs_RoundProgramId",
                        column: x => x.RoundProgramId,
                        principalTable: "round_programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "round_document_requirements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoundProgramId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    RequiresNotarization = table.Column<bool>(type: "bit", nullable: false),
                    RequiresOriginalCopy = table.Column<bool>(type: "bit", nullable: false),
                    MaxFiles = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_round_document_requirements", x => x.Id);
                    table.CheckConstraint("ck_round_document_requirements_max_files", "[MaxFiles] > 0");
                    table.ForeignKey(
                        name: "FK_round_document_requirements_document_types_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalTable: "document_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_round_document_requirements_round_programs_RoundProgramId",
                        column: x => x.RoundProgramId,
                        principalTable: "round_programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "application_documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StoragePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Checksum = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    UploadedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ValidationStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsLatest = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_application_documents", x => x.Id);
                    table.CheckConstraint("ck_application_documents_file_size", "[FileSize] >= 0");
                    table.ForeignKey(
                        name: "FK_application_documents_admission_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "admission_applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_application_documents_document_types_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalTable: "document_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_application_documents_users_UploadedBy",
                        column: x => x.UploadedBy,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "application_preferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PriorityOrder = table.Column<int>(type: "int", nullable: false),
                    ProgramId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MajorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MethodId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_application_preferences", x => x.Id);
                    table.CheckConstraint("ck_application_preferences_priority_order", "[PriorityOrder] > 0");
                    table.ForeignKey(
                        name: "FK_application_preferences_admission_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "admission_applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_application_preferences_admission_methods_MethodId",
                        column: x => x.MethodId,
                        principalTable: "admission_methods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_application_preferences_majors_MajorId",
                        column: x => x.MajorId,
                        principalTable: "majors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_application_preferences_training_programs_ProgramId",
                        column: x => x.ProgramId,
                        principalTable: "training_programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "application_review_notes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NoteType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsVisibleToCandidate = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_application_review_notes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_application_review_notes_admission_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "admission_applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_application_review_notes_users_AuthorUserId",
                        column: x => x.AuthorUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "application_status_histories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ToStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ChangedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PublicNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InternalNote = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_application_status_histories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_application_status_histories_admission_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "admission_applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_application_status_histories_users_ChangedBy",
                        column: x => x.ChangedBy,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "application_supplement_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DueAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RequestContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_application_supplement_requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_application_supplement_requests_admission_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "admission_applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_application_supplement_requests_users_RequestedBy",
                        column: x => x.RequestedBy,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "enrollment_confirmations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConfirmedByCandidateAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConfirmationStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enrollment_confirmations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_enrollment_confirmations_admission_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "admission_applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Channel = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_notifications_admission_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "admission_applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notifications_notification_templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "notification_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notifications_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "admission_methods",
                columns: new[] { "Id", "CreatedAt", "Description", "MethodCode", "MethodName", "Status", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-1111-1111-1111-111111111111"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Academic transcript based admission", "HOC_BA", "Xet hoc ba", "ACTIVE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("bbbbbbbb-2222-2222-2222-222222222222"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Exam score based admission", "DIEM_THI", "Xet diem thi", "ACTIVE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("cccccccc-3333-3333-3333-333333333333"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Direct admission", "TUYEN_THANG", "Xet tuyen thang", "ACTIVE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "admission_rounds",
                columns: new[] { "Id", "AdmissionYear", "AllowEnrollmentConfirmation", "CreatedAt", "CreatedBy", "DeletedAt", "EndAt", "Notes", "RoundCode", "RoundName", "StartAt", "Status", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("aaaaaaa1-1111-1111-1111-111111111111"), 2026, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, new DateTime(2026, 6, 30, 23, 59, 59, 0, DateTimeKind.Utc), "Dot xet tuyen chinh quy nam 2026", "DOT1-2026", "Dot 1 nam 2026", new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), "PUBLISHED", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("bbbbbbb1-1111-1111-1111-111111111111"), 2025, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, new DateTime(2025, 6, 30, 23, 59, 59, 0, DateTimeKind.Utc), "Dot xet tuyen chinh quy nam 2025", "DOT1-2025", "Dot 1 nam 2025", new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), "PUBLISHED", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("bbbbbbb2-1111-1111-1111-111111111111"), 2026, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, new DateTime(2026, 9, 30, 23, 59, 59, 0, DateTimeKind.Utc), "Dot xet tuyen chinh quy dot 2 nam 2026", "DOT2-2026", "Dot 2 nam 2026", new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), "PUBLISHED", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("bbbbbbb3-1111-1111-1111-111111111111"), 2027, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, new DateTime(2027, 6, 30, 23, 59, 59, 0, DateTimeKind.Utc), "Dot xet tuyen chinh quy nam 2027", "DOT3-2027", "Dot 3 nam 2027", new DateTime(2027, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), "PUBLISHED", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "document_types",
                columns: new[] { "Id", "CreatedAt", "Description", "DocumentCode", "DocumentName", "Status" },
                values: new object[,]
                {
                    { new Guid("dddddddd-4444-4444-4444-444444444444"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Citizen identification card", "CCCD", "Can cuoc cong dan", "ACTIVE" },
                    { new Guid("eeeeeeee-5555-5555-5555-555555555555"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Academic transcript", "HOC_BA", "Hoc ba", "ACTIVE" },
                    { new Guid("ffffffff-6666-6666-6666-666666666666"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Birth certificate", "KHAI_SINH", "Giay khai sinh", "ACTIVE" }
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "Id", "Code", "CreatedAt", "Description", "IsSystemRole", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "ADMIN", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System administrator", true, "Administrator", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "ADMISSION_OFFICER", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Admission operations staff", true, "Admission Officer", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "REPORT_VIEWER", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Read-only reporting role", true, "Report Viewer", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "CANDIDATE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Candidate account role", true, "Candidate", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "system_configs",
                columns: new[] { "Id", "ConfigKey", "ConfigValue", "Description", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("88888888-8888-8888-8888-888888888881"), "AUTH.LOGIN_BY_EMAIL", "true", "Allow login by email", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("88888888-8888-8888-8888-888888888882"), "AUTH.LOGIN_BY_PHONE", "true", "Allow login by phone", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null }
                });

            migrationBuilder.InsertData(
                table: "training_programs",
                columns: new[] { "Id", "CreatedAt", "Description", "DisplayOrder", "DurationText", "EducationType", "ManagingUnit", "ProgramCode", "ProgramName", "Quota", "Status", "TuitionFee", "UpdatedAt" },
                values: new object[] { new Guid("99999999-7777-7777-7777-777777777777"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Default seeded training program", 1, "2.5 years", "CAO_DANG", "Admissions Office", "CD_CHINH_QUY", "Cao dang chinh quy", 500, "ACTIVE", 12000000m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

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

            migrationBuilder.CreateIndex(
                name: "IX_admission_applications_ApplicationCode",
                table: "admission_applications",
                column: "ApplicationCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_admission_applications_CandidateId_RoundProgramId",
                table: "admission_applications",
                columns: new[] { "CandidateId", "RoundProgramId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_admission_applications_CurrentStatus_SubmittedAt",
                table: "admission_applications",
                columns: new[] { "CurrentStatus", "SubmittedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_admission_applications_RoundProgramId",
                table: "admission_applications",
                column: "RoundProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_admission_methods_MethodCode",
                table: "admission_methods",
                column: "MethodCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_admission_rounds_AdmissionYear_Status_StartAt_EndAt",
                table: "admission_rounds",
                columns: new[] { "AdmissionYear", "Status", "StartAt", "EndAt" });

            migrationBuilder.CreateIndex(
                name: "IX_admission_rounds_CreatedBy",
                table: "admission_rounds",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_admission_rounds_RoundCode",
                table: "admission_rounds",
                column: "RoundCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_application_documents_ApplicationId_DocumentTypeId_IsLatest",
                table: "application_documents",
                columns: new[] { "ApplicationId", "DocumentTypeId", "IsLatest" });

            migrationBuilder.CreateIndex(
                name: "IX_application_documents_DocumentTypeId",
                table: "application_documents",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_application_documents_UploadedBy",
                table: "application_documents",
                column: "UploadedBy");

            migrationBuilder.CreateIndex(
                name: "IX_application_preferences_ApplicationId_PriorityOrder",
                table: "application_preferences",
                columns: new[] { "ApplicationId", "PriorityOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_application_preferences_MajorId",
                table: "application_preferences",
                column: "MajorId");

            migrationBuilder.CreateIndex(
                name: "IX_application_preferences_MethodId",
                table: "application_preferences",
                column: "MethodId");

            migrationBuilder.CreateIndex(
                name: "IX_application_preferences_ProgramId",
                table: "application_preferences",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_application_review_notes_ApplicationId",
                table: "application_review_notes",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_application_review_notes_AuthorUserId",
                table: "application_review_notes",
                column: "AuthorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_application_status_histories_ApplicationId_ChangedAt",
                table: "application_status_histories",
                columns: new[] { "ApplicationId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_application_status_histories_ChangedBy",
                table: "application_status_histories",
                column: "ChangedBy");

            migrationBuilder.CreateIndex(
                name: "IX_application_supplement_requests_ApplicationId",
                table: "application_supplement_requests",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_application_supplement_requests_RequestedBy",
                table: "application_supplement_requests",
                column: "RequestedBy");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_ActorUserId",
                table: "audit_logs",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_EntityName_EntityId_CreatedAt",
                table: "audit_logs",
                columns: new[] { "EntityName", "EntityId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_auth_logs_LoggedAt",
                table: "auth_logs",
                column: "LoggedAt");

            migrationBuilder.CreateIndex(
                name: "IX_auth_logs_UserId",
                table: "auth_logs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_candidate_education_profiles_CandidateId",
                table: "candidate_education_profiles",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_candidate_priority_profiles_CandidateId",
                table: "candidate_priority_profiles",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_candidates_NationalId",
                table: "candidates",
                column: "NationalId",
                unique: true,
                filter: "[NationalId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_candidates_ProvinceCode",
                table: "candidates",
                column: "ProvinceCode");

            migrationBuilder.CreateIndex(
                name: "IX_candidates_UserId",
                table: "candidates",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_document_types_DocumentCode",
                table: "document_types",
                column: "DocumentCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_enrollment_confirmations_ApplicationId",
                table: "enrollment_confirmations",
                column: "ApplicationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_majors_MajorCode",
                table: "majors",
                column: "MajorCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_majors_ProgramId",
                table: "majors",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_notification_templates_TemplateCode",
                table: "notification_templates",
                column: "TemplateCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notifications_ApplicationId",
                table: "notifications",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_TemplateId",
                table: "notifications",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_UserId_Status_CreatedAt",
                table: "notifications",
                columns: new[] { "UserId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_password_reset_tokens_Token",
                table: "password_reset_tokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_password_reset_tokens_UserId",
                table: "password_reset_tokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_roles_Code",
                table: "roles",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_round_admission_methods_MethodId",
                table: "round_admission_methods",
                column: "MethodId");

            migrationBuilder.CreateIndex(
                name: "IX_round_admission_methods_RoundProgramId",
                table: "round_admission_methods",
                column: "RoundProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_round_document_requirements_DocumentTypeId",
                table: "round_document_requirements",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_round_document_requirements_RoundProgramId_DocumentTypeId",
                table: "round_document_requirements",
                columns: new[] { "RoundProgramId", "DocumentTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_round_programs_MajorId",
                table: "round_programs",
                column: "MajorId");

            migrationBuilder.CreateIndex(
                name: "IX_round_programs_ProgramId",
                table: "round_programs",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_round_programs_RoundId_ProgramId_MajorId",
                table: "round_programs",
                columns: new[] { "RoundId", "ProgramId", "MajorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_system_configs_ConfigKey",
                table: "system_configs",
                column: "ConfigKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_system_configs_UpdatedBy",
                table: "system_configs",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_training_programs_ProgramCode",
                table: "training_programs",
                column: "ProgramCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_AssignedBy",
                table: "user_roles",
                column: "AssignedBy");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_RoleId",
                table: "user_roles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_UserId_RoleId",
                table: "user_roles",
                columns: new[] { "UserId", "RoleId" },
                unique: true,
                filter: "[RevokedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_UserId_RoleId_RevokedAt",
                table: "user_roles",
                columns: new[] { "UserId", "RoleId", "RevokedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_users_PhoneNumber",
                table: "users",
                column: "PhoneNumber",
                unique: true,
                filter: "[PhoneNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_users_Status",
                table: "users",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "application_documents");

            migrationBuilder.DropTable(
                name: "application_preferences");

            migrationBuilder.DropTable(
                name: "application_review_notes");

            migrationBuilder.DropTable(
                name: "application_status_histories");

            migrationBuilder.DropTable(
                name: "application_supplement_requests");

            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "auth_logs");

            migrationBuilder.DropTable(
                name: "candidate_education_profiles");

            migrationBuilder.DropTable(
                name: "candidate_priority_profiles");

            migrationBuilder.DropTable(
                name: "enrollment_confirmations");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "password_reset_tokens");

            migrationBuilder.DropTable(
                name: "round_admission_methods");

            migrationBuilder.DropTable(
                name: "round_document_requirements");

            migrationBuilder.DropTable(
                name: "system_configs");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "admission_applications");

            migrationBuilder.DropTable(
                name: "notification_templates");

            migrationBuilder.DropTable(
                name: "admission_methods");

            migrationBuilder.DropTable(
                name: "document_types");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "candidates");

            migrationBuilder.DropTable(
                name: "round_programs");

            migrationBuilder.DropTable(
                name: "admission_rounds");

            migrationBuilder.DropTable(
                name: "majors");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "training_programs");
        }
    }
}
