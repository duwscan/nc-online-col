using Microsoft.EntityFrameworkCore;
using wnc.Models;

namespace wnc.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<AuthLog> AuthLogs => Set<AuthLog>();
    public DbSet<Candidate> Candidates => Set<Candidate>();
    public DbSet<CandidateEducationProfile> CandidateEducationProfiles => Set<CandidateEducationProfile>();
    public DbSet<CandidatePriorityProfile> CandidatePriorityProfiles => Set<CandidatePriorityProfile>();
    public DbSet<TrainingProgram> TrainingPrograms => Set<TrainingProgram>();
    public DbSet<Major> Majors => Set<Major>();
    public DbSet<AdmissionRound> AdmissionRounds => Set<AdmissionRound>();
    public DbSet<AdmissionMethod> AdmissionMethods => Set<AdmissionMethod>();
    public DbSet<RoundProgram> RoundPrograms => Set<RoundProgram>();
    public DbSet<RoundAdmissionMethod> RoundAdmissionMethods => Set<RoundAdmissionMethod>();
    public DbSet<DocumentType> DocumentTypes => Set<DocumentType>();
    public DbSet<RoundDocumentRequirement> RoundDocumentRequirements => Set<RoundDocumentRequirement>();
    public DbSet<AdmissionApplication> AdmissionApplications => Set<AdmissionApplication>();
    public DbSet<ApplicationPreference> ApplicationPreferences => Set<ApplicationPreference>();
    public DbSet<ApplicationDocument> ApplicationDocuments => Set<ApplicationDocument>();
    public DbSet<ApplicationStatusHistory> ApplicationStatusHistories => Set<ApplicationStatusHistory>();
    public DbSet<ApplicationReviewNote> ApplicationReviewNotes => Set<ApplicationReviewNote>();
    public DbSet<ApplicationSupplementRequest> ApplicationSupplementRequests => Set<ApplicationSupplementRequest>();
    public DbSet<EnrollmentConfirmation> EnrollmentConfirmations => Set<EnrollmentConfirmation>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SystemConfig> SystemConfigs => Set<SystemConfig>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureIdentityAccess(modelBuilder);
        ConfigureCandidateManagement(modelBuilder);
        ConfigureAdmissionCatalog(modelBuilder);
        ConfigureApplicationProcessing(modelBuilder);
        ConfigureCommunicationAudit(modelBuilder);

        SeedLookupData(modelBuilder);
    }

    private static void ConfigureIdentityAccess(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasIndex(x => x.PhoneNumber).IsUnique();
            entity.HasIndex(x => x.Status);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(x => x.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(x => x.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("user_roles");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UserId, x.RoleId })
                .IsUnique()
                .HasFilter("[RevokedAt] IS NULL");
            entity.HasIndex(x => new { x.UserId, x.RoleId, x.RevokedAt });
            entity.Property(x => x.AssignedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(x => x.User)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Role)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.AssignedByUser)
                .WithMany(x => x.AssignedRoles)
                .HasForeignKey(x => x.AssignedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.ToTable("password_reset_tokens");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Token).IsUnique();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(x => x.User)
                .WithMany(x => x.PasswordResetTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuthLog>(entity =>
        {
            entity.ToTable("auth_logs");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.LoggedAt);
            entity.Property(x => x.LoggedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(x => x.User)
                .WithMany(x => x.AuthLogs)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureCandidateManagement(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Candidate>(entity =>
        {
            entity.ToTable("candidates");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.UserId).IsUnique();
            entity.HasIndex(x => x.NationalId).IsUnique();
            entity.HasIndex(x => x.ProvinceCode);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(x => x.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(x => x.User)
                .WithOne(x => x.Candidate)
                .HasForeignKey<Candidate>(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CandidateEducationProfile>(entity =>
        {
            entity.ToTable("candidate_education_profiles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Gpa).HasPrecision(5, 2);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(x => x.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(x => x.Candidate)
                .WithMany(x => x.EducationProfiles)
                .HasForeignKey(x => x.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CandidatePriorityProfile>(entity =>
        {
            entity.ToTable("candidate_priority_profiles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ScoreValue).HasPrecision(5, 2);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(x => x.Candidate)
                .WithMany(x => x.PriorityProfiles)
                .HasForeignKey(x => x.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureAdmissionCatalog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TrainingProgram>(entity =>
        {
            entity.ToTable("training_programs", t => t.HasCheckConstraint("ck_training_programs_quota", "[Quota] >= 0"));
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.ProgramCode).IsUnique();
            entity.Property(x => x.TuitionFee).HasPrecision(12, 2);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(x => x.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<Major>(entity =>
        {
            entity.ToTable("majors", t => t.HasCheckConstraint("ck_majors_quota", "[Quota] >= 0"));
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.MajorCode).IsUnique();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(x => x.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(x => x.Program)
                .WithMany(x => x.Majors)
                .HasForeignKey(x => x.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AdmissionRound>(entity =>
        {
            entity.ToTable("admission_rounds", t => t.HasCheckConstraint("ck_admission_rounds_time", "[StartAt] < [EndAt]"));
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.RoundCode).IsUnique();
            entity.HasIndex(x => new { x.AdmissionYear, x.Status, x.StartAt, x.EndAt });
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(x => x.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(x => x.CreatedByUser)
                .WithMany(x => x.CreatedAdmissionRounds)
                .HasForeignKey(x => x.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AdmissionMethod>(entity =>
        {
            entity.ToTable("admission_methods");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.MethodCode).IsUnique();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(x => x.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<RoundProgram>(entity =>
        {
            entity.ToTable("round_programs", t =>
            {
                t.HasCheckConstraint("ck_round_programs_quota", "[Quota] >= 0");
                t.HasCheckConstraint("ck_round_programs_published_quota", "[PublishedQuota] IS NULL OR [PublishedQuota] >= 0");
            });
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.RoundId, x.ProgramId, x.MajorId })
                .IsUnique()
                .HasFilter(null);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(x => x.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(x => x.Round)
                .WithMany(x => x.RoundPrograms)
                .HasForeignKey(x => x.RoundId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Program)
                .WithMany(x => x.RoundPrograms)
                .HasForeignKey(x => x.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Major)
                .WithMany(x => x.RoundPrograms)
                .HasForeignKey(x => x.MajorId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(x => x.AdmissionMethods)
                .WithOne(x => x.RoundProgram)
                .HasForeignKey(x => x.RoundProgramId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RoundAdmissionMethod>(entity =>
        {
            entity.ToTable("round_admission_methods");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.MinimumScore).HasPrecision(5, 2);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(x => x.Method)
                .WithMany(x => x.RoundAdmissionMethods)
                .HasForeignKey(x => x.MethodId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DocumentType>(entity =>
        {
            entity.ToTable("document_types");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.DocumentCode).IsUnique();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasMany(x => x.RoundDocumentRequirements)
                .WithOne(x => x.DocumentType)
                .HasForeignKey(x => x.DocumentTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RoundDocumentRequirement>(entity =>
        {
            entity.ToTable("round_document_requirements", t => t.HasCheckConstraint("ck_round_document_requirements_max_files", "[MaxFiles] > 0"));
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.RoundProgramId, x.DocumentTypeId }).IsUnique();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(x => x.RoundProgram)
                .WithMany(x => x.DocumentRequirements)
                .HasForeignKey(x => x.RoundProgramId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureApplicationProcessing(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdmissionApplication>(entity =>
        {
            entity.ToTable("admission_applications", t => t.HasCheckConstraint("ck_admission_applications_submission_number", "[SubmissionNumber] >= 0"));
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.ApplicationCode).IsUnique();
            entity.HasIndex(x => new { x.CandidateId, x.RoundProgramId }).IsUnique();
            entity.HasIndex(x => new { x.CurrentStatus, x.SubmittedAt });
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(x => x.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(x => x.Candidate)
                .WithMany(x => x.AdmissionApplications)
                .HasForeignKey(x => x.CandidateId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RoundProgram)
                .WithMany(x => x.AdmissionApplications)
                .HasForeignKey(x => x.RoundProgramId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ApplicationPreference>(entity =>
        {
            entity.ToTable("application_preferences", t => t.HasCheckConstraint("ck_application_preferences_priority_order", "[PriorityOrder] > 0"));
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.ApplicationId, x.PriorityOrder }).IsUnique();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(x => x.Application)
                .WithMany(x => x.ApplicationPreferences)
                .HasForeignKey(x => x.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Program)
                .WithMany(x => x.ApplicationPreferences)
                .HasForeignKey(x => x.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Major)
                .WithMany(x => x.ApplicationPreferences)
                .HasForeignKey(x => x.MajorId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Method)
                .WithMany(x => x.ApplicationPreferences)
                .HasForeignKey(x => x.MethodId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ApplicationDocument>(entity =>
        {
            entity.ToTable("application_documents", t => t.HasCheckConstraint("ck_application_documents_file_size", "[FileSize] >= 0"));
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.ApplicationId, x.DocumentTypeId, x.IsLatest });
            entity.Property(x => x.UploadedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(x => x.Application)
                .WithMany(x => x.ApplicationDocuments)
                .HasForeignKey(x => x.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.DocumentType)
                .WithMany(x => x.ApplicationDocuments)
                .HasForeignKey(x => x.DocumentTypeId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.UploadedByUser)
                .WithMany(x => x.UploadedDocuments)
                .HasForeignKey(x => x.UploadedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ApplicationStatusHistory>(entity =>
        {
            entity.ToTable("application_status_histories");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.ApplicationId, x.ChangedAt });
            entity.Property(x => x.ChangedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(x => x.Application)
                .WithMany(x => x.ApplicationStatusHistories)
                .HasForeignKey(x => x.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.ChangedByUser)
                .WithMany(x => x.ApplicationStatusHistories)
                .HasForeignKey(x => x.ChangedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ApplicationReviewNote>(entity =>
        {
            entity.ToTable("application_review_notes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(x => x.Application)
                .WithMany(x => x.ApplicationReviewNotes)
                .HasForeignKey(x => x.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.AuthorUser)
                .WithMany(x => x.ApplicationReviewNotes)
                .HasForeignKey(x => x.AuthorUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ApplicationSupplementRequest>(entity =>
        {
            entity.ToTable("application_supplement_requests");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RequestedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(x => x.Application)
                .WithMany(x => x.ApplicationSupplementRequests)
                .HasForeignKey(x => x.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.RequestedByUser)
                .WithMany(x => x.ApplicationSupplementRequests)
                .HasForeignKey(x => x.RequestedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EnrollmentConfirmation>(entity =>
        {
            entity.ToTable("enrollment_confirmations");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.ApplicationId).IsUnique();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(x => x.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(x => x.Application)
                .WithOne(x => x.EnrollmentConfirmation)
                .HasForeignKey<EnrollmentConfirmation>(x => x.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureCommunicationAudit(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UserId, x.Status, x.CreatedAt });
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(x => x.User)
                .WithMany(x => x.Notifications)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Application)
                .WithMany(x => x.Notifications)
                .HasForeignKey(x => x.ApplicationId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Template)
                .WithMany(x => x.Notifications)
                .HasForeignKey(x => x.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<NotificationTemplate>(entity =>
        {
            entity.ToTable("notification_templates");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TemplateCode).IsUnique();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(x => x.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_logs");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.EntityName, x.EntityId, x.CreatedAt });
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(x => x.ActorUser)
                .WithMany(x => x.AuditLogs)
                .HasForeignKey(x => x.ActorUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SystemConfig>(entity =>
        {
            entity.ToTable("system_configs");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.ConfigKey).IsUnique();
            entity.Property(x => x.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(x => x.UpdatedByUser)
                .WithMany(x => x.UpdatedSystemConfigs)
                .HasForeignKey(x => x.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void SeedLookupData(ModelBuilder modelBuilder)
    {
        var seededAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Role>().HasData(
            new Role
            {
                Id = SeedIds.AdminRoleId,
                Code = "ADMIN",
                Name = "Administrator",
                Description = "System administrator",
                IsSystemRole = true,
                CreatedAt = seededAt,
                UpdatedAt = seededAt
            },
            new Role
            {
                Id = SeedIds.AdmissionOfficerRoleId,
                Code = "ADMISSION_OFFICER",
                Name = "Admission Officer",
                Description = "Admission operations staff",
                IsSystemRole = true,
                CreatedAt = seededAt,
                UpdatedAt = seededAt
            },
            new Role
            {
                Id = SeedIds.ReportViewerRoleId,
                Code = "REPORT_VIEWER",
                Name = "Report Viewer",
                Description = "Read-only reporting role",
                IsSystemRole = true,
                CreatedAt = seededAt,
                UpdatedAt = seededAt
            },
            new Role
            {
                Id = SeedIds.CandidateRoleId,
                Code = "CANDIDATE",
                Name = "Candidate",
                Description = "Candidate account role",
                IsSystemRole = true,
                CreatedAt = seededAt,
                UpdatedAt = seededAt
            });

        modelBuilder.Entity<AdmissionMethod>().HasData(
            new AdmissionMethod
            {
                Id = SeedIds.TranscriptMethodId,
                MethodCode = "HOC_BA",
                MethodName = "Xet hoc ba",
                Description = "Academic transcript based admission",
                Status = "ACTIVE",
                CreatedAt = seededAt,
                UpdatedAt = seededAt
            },
            new AdmissionMethod
            {
                Id = SeedIds.ExamMethodId,
                MethodCode = "DIEM_THI",
                MethodName = "Xet diem thi",
                Description = "Exam score based admission",
                Status = "ACTIVE",
                CreatedAt = seededAt,
                UpdatedAt = seededAt
            },
            new AdmissionMethod
            {
                Id = SeedIds.DirectMethodId,
                MethodCode = "TUYEN_THANG",
                MethodName = "Xet tuyen thang",
                Description = "Direct admission",
                Status = "ACTIVE",
                CreatedAt = seededAt,
                UpdatedAt = seededAt
            });

        modelBuilder.Entity<DocumentType>().HasData(
            new DocumentType
            {
                Id = SeedIds.IdCardDocumentId,
                DocumentCode = "CCCD",
                DocumentName = "Can cuoc cong dan",
                Description = "Citizen identification card",
                Status = "ACTIVE",
                CreatedAt = seededAt
            },
            new DocumentType
            {
                Id = SeedIds.TranscriptDocumentId,
                DocumentCode = "HOC_BA",
                DocumentName = "Hoc ba",
                Description = "Academic transcript",
                Status = "ACTIVE",
                CreatedAt = seededAt
            },
            new DocumentType
            {
                Id = SeedIds.BirthCertificateDocumentId,
                DocumentCode = "KHAI_SINH",
                DocumentName = "Giay khai sinh",
                Description = "Birth certificate",
                Status = "ACTIVE",
                CreatedAt = seededAt
            });

        modelBuilder.Entity<TrainingProgram>().HasData(
            new TrainingProgram
            {
                Id = SeedIds.DefaultProgramId,
                ProgramCode = "CD_CHINH_QUY",
                ProgramName = "Cao dang chinh quy",
                EducationType = "CAO_DANG",
                Description = "Default seeded training program",
                TuitionFee = 12000000m,
                DurationText = "2.5 years",
                Quota = 500,
                ManagingUnit = "Admissions Office",
                Status = "ACTIVE",
                DisplayOrder = 1,
                CreatedAt = seededAt,
                UpdatedAt = seededAt
            });

        modelBuilder.Entity<Major>().HasData(
            new Major
            {
                Id = SeedIds.MajorCnttId,
                ProgramId = SeedIds.DefaultProgramId,
                MajorCode = "CNTT",
                MajorName = "Cong nghe thong tin",
                Description = "Cong nghe thong tin",
                Quota = 150,
                DisplayOrder = 1,
                Status = "ACTIVE",
                CreatedAt = seededAt,
                UpdatedAt = seededAt
            },
            new Major
            {
                Id = SeedIds.MajorKeToanId,
                ProgramId = SeedIds.DefaultProgramId,
                MajorCode = "KT",
                MajorName = "Ke toan",
                Description = "Ke toan doanh nghiep",
                Quota = 100,
                DisplayOrder = 2,
                Status = "ACTIVE",
                CreatedAt = seededAt,
                UpdatedAt = seededAt
            },
            new Major
            {
                Id = SeedIds.MajorMarketingId,
                ProgramId = SeedIds.DefaultProgramId,
                MajorCode = "MK",
                MajorName = "Marketing",
                Description = "Marketing so",
                Quota = 80,
                DisplayOrder = 3,
                Status = "ACTIVE",
                CreatedAt = seededAt,
                UpdatedAt = seededAt
            });

        modelBuilder.Entity<AdmissionRound>().HasData(
            new AdmissionRound
            {
                Id = SeedIds.Round2025Id,
                RoundCode = "DOT1-2025",
                RoundName = "Dot 1 nam 2025",
                AdmissionYear = 2025,
                StartAt = new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                EndAt = new DateTime(2025, 6, 30, 23, 59, 59, DateTimeKind.Utc),
                Status = "PUBLISHED",
                Notes = "Dot xet tuyen chinh quy nam 2025",
                AllowEnrollmentConfirmation = false,
                CreatedAt = seededAt,
                UpdatedAt = seededAt
            },
            new AdmissionRound
            {
                Id = SeedIds.Round2026Id,
                RoundCode = "DOT1-2026",
                RoundName = "Dot 1 nam 2026",
                AdmissionYear = 2026,
                StartAt = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                EndAt = new DateTime(2026, 6, 30, 23, 59, 59, DateTimeKind.Utc),
                Status = "PUBLISHED",
                Notes = "Dot xet tuyen chinh quy nam 2026",
                AllowEnrollmentConfirmation = true,
                CreatedAt = seededAt,
                UpdatedAt = seededAt
            },
            new AdmissionRound
            {
                Id = SeedIds.Round2026Dot2Id,
                RoundCode = "DOT2-2026",
                RoundName = "Dot 2 nam 2026",
                AdmissionYear = 2026,
                StartAt = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc),
                EndAt = new DateTime(2026, 9, 30, 23, 59, 59, DateTimeKind.Utc),
                Status = "PUBLISHED",
                Notes = "Dot xet tuyen chinh quy dot 2 nam 2026",
                AllowEnrollmentConfirmation = false,
                CreatedAt = seededAt,
                UpdatedAt = seededAt
            },
            new AdmissionRound
            {
                Id = SeedIds.Round2026Dot3Id,
                RoundCode = "DOT3-2027",
                RoundName = "Dot 3 nam 2027",
                AdmissionYear = 2027,
                StartAt = new DateTime(2027, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                EndAt = new DateTime(2027, 6, 30, 23, 59, 59, DateTimeKind.Utc),
                Status = "PUBLISHED",
                Notes = "Dot xet tuyen chinh quy nam 2027",
                AllowEnrollmentConfirmation = false,
                CreatedAt = seededAt,
                UpdatedAt = seededAt
            });

        modelBuilder.Entity<RoundProgram>().HasData(
            new RoundProgram
            {
                Id = SeedIds.RoundProgramCnttId,
                RoundId = SeedIds.Round2026Id,
                ProgramId = SeedIds.DefaultProgramId,
                MajorId = SeedIds.MajorCnttId,
                Quota = 150,
                PublishedQuota = 120,
                Status = "ACTIVE",
                CreatedAt = seededAt,
                UpdatedAt = seededAt
            },
            new RoundProgram
            {
                Id = SeedIds.RoundProgramKeToanId,
                RoundId = SeedIds.Round2026Id,
                ProgramId = SeedIds.DefaultProgramId,
                MajorId = SeedIds.MajorKeToanId,
                Quota = 100,
                PublishedQuota = 80,
                Status = "ACTIVE",
                CreatedAt = seededAt,
                UpdatedAt = seededAt
            },
            new RoundProgram
            {
                Id = SeedIds.RoundProgramMarketingId,
                RoundId = SeedIds.Round2026Id,
                ProgramId = SeedIds.DefaultProgramId,
                MajorId = SeedIds.MajorMarketingId,
                Quota = 80,
                PublishedQuota = 60,
                Status = "ACTIVE",
                CreatedAt = seededAt,
                UpdatedAt = seededAt
            },
            new RoundProgram
            {
                Id = SeedIds.RoundProgram2025CnttId,
                RoundId = SeedIds.Round2025Id,
                ProgramId = SeedIds.DefaultProgramId,
                MajorId = SeedIds.MajorCnttId,
                Quota = 100,
                PublishedQuota = 90,
                Status = "ACTIVE",
                CreatedAt = seededAt,
                UpdatedAt = seededAt
            },
            new RoundProgram
            {
                Id = SeedIds.RoundProgram2025KtId,
                RoundId = SeedIds.Round2025Id,
                ProgramId = SeedIds.DefaultProgramId,
                MajorId = SeedIds.MajorKeToanId,
                Quota = 80,
                PublishedQuota = 70,
                Status = "ACTIVE",
                CreatedAt = seededAt,
                UpdatedAt = seededAt
            },
            new RoundProgram
            {
                Id = SeedIds.RoundProgram2026Dot2CnttId,
                RoundId = SeedIds.Round2026Dot2Id,
                ProgramId = SeedIds.DefaultProgramId,
                MajorId = SeedIds.MajorCnttId,
                Quota = 100,
                PublishedQuota = 80,
                Status = "ACTIVE",
                CreatedAt = seededAt,
                UpdatedAt = seededAt
            },
            new RoundProgram
            {
                Id = SeedIds.RoundProgram2026Dot3CnttId,
                RoundId = SeedIds.Round2026Dot3Id,
                ProgramId = SeedIds.DefaultProgramId,
                MajorId = SeedIds.MajorCnttId,
                Quota = 120,
                PublishedQuota = 100,
                Status = "ACTIVE",
                CreatedAt = seededAt,
                UpdatedAt = seededAt
            });

        modelBuilder.Entity<RoundAdmissionMethod>().HasData(
            new RoundAdmissionMethod
            {
                Id = SeedIds.RoundMethodCnttHocBaId,
                RoundProgramId = SeedIds.RoundProgramCnttId,
                MethodId = SeedIds.TranscriptMethodId,
                MinimumScore = 7.0m,
                CreatedAt = seededAt
            },
            new RoundAdmissionMethod
            {
                Id = SeedIds.RoundMethodCnttDiemThiId,
                RoundProgramId = SeedIds.RoundProgramCnttId,
                MethodId = SeedIds.ExamMethodId,
                MinimumScore = 18.0m,
                CreatedAt = seededAt
            },
            new RoundAdmissionMethod
            {
                Id = SeedIds.RoundMethodKeToanHocBaId,
                RoundProgramId = SeedIds.RoundProgramKeToanId,
                MethodId = SeedIds.TranscriptMethodId,
                MinimumScore = 6.5m,
                CreatedAt = seededAt
            },
            new RoundAdmissionMethod
            {
                Id = SeedIds.RoundMethodMarketingHocBaId,
                RoundProgramId = SeedIds.RoundProgramMarketingId,
                MethodId = SeedIds.TranscriptMethodId,
                MinimumScore = 6.0m,
                CreatedAt = seededAt
            });

        modelBuilder.Entity<SystemConfig>().HasData(
            new SystemConfig
            {
                Id = SeedIds.LoginByEmailConfigId,
                ConfigKey = "AUTH.LOGIN_BY_EMAIL",
                ConfigValue = "true",
                Description = "Allow login by email",
                UpdatedBy = null,
                UpdatedAt = seededAt
            },
            new SystemConfig
            {
                Id = SeedIds.LoginByPhoneConfigId,
                ConfigKey = "AUTH.LOGIN_BY_PHONE",
                ConfigValue = "true",
                Description = "Allow login by phone",
                UpdatedBy = null,
                UpdatedAt = seededAt
            });
    }
}

public static class SeedIds
{
    public static readonly Guid AdminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid AdmissionOfficerRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid ReportViewerRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid CandidateRoleId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    public static readonly Guid TranscriptMethodId = Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111");
    public static readonly Guid ExamMethodId = Guid.Parse("bbbbbbbb-2222-2222-2222-222222222222");
    public static readonly Guid DirectMethodId = Guid.Parse("cccccccc-3333-3333-3333-333333333333");

    public static readonly Guid IdCardDocumentId = Guid.Parse("dddddddd-4444-4444-4444-444444444444");
    public static readonly Guid TranscriptDocumentId = Guid.Parse("eeeeeeee-5555-5555-5555-555555555555");
    public static readonly Guid BirthCertificateDocumentId = Guid.Parse("ffffffff-6666-6666-6666-666666666666");

    public static readonly Guid DefaultProgramId = Guid.Parse("99999999-7777-7777-7777-777777777777");

    public static readonly Guid LoginByEmailConfigId = Guid.Parse("88888888-8888-8888-8888-888888888881");
    public static readonly Guid LoginByPhoneConfigId = Guid.Parse("88888888-8888-8888-8888-888888888882");

    public static readonly Guid Round2026Id = Guid.Parse("aaaaaaa1-1111-1111-1111-111111111111");
    public static readonly Guid MajorCnttId = Guid.Parse("aaaaaaa2-1111-1111-1111-111111111111");
    public static readonly Guid MajorKeToanId = Guid.Parse("aaaaaaa3-1111-1111-1111-111111111111");
    public static readonly Guid MajorMarketingId = Guid.Parse("aaaaaaa4-1111-1111-1111-111111111111");
    public static readonly Guid RoundProgramCnttId = Guid.Parse("aaaaaaa5-1111-1111-1111-111111111111");
    public static readonly Guid RoundProgramKeToanId = Guid.Parse("aaaaaaa6-1111-1111-1111-111111111111");
    public static readonly Guid RoundProgramMarketingId = Guid.Parse("aaaaaaa7-1111-1111-1111-111111111111");
    public static readonly Guid RoundMethodCnttHocBaId = Guid.Parse("aaaaaa01-1111-1111-1111-111111111111");
    public static readonly Guid RoundMethodCnttDiemThiId = Guid.Parse("aaaaaa02-1111-1111-1111-111111111111");
    public static readonly Guid RoundMethodKeToanHocBaId = Guid.Parse("aaaaaa03-1111-1111-1111-111111111111");
    public static readonly Guid RoundMethodMarketingHocBaId = Guid.Parse("aaaaaa04-1111-1111-1111-111111111111");

    // Additional rounds
    public static readonly Guid Round2025Id = Guid.Parse("bbbbbbb1-1111-1111-1111-111111111111");
    public static readonly Guid Round2026Dot2Id = Guid.Parse("bbbbbbb2-1111-1111-1111-111111111111");
    public static readonly Guid Round2026Dot3Id = Guid.Parse("bbbbbbb3-1111-1111-1111-111111111111");
    public static readonly Guid RoundProgram2025CnttId = Guid.Parse("bbbbbb01-1111-1111-1111-111111111111");
    public static readonly Guid RoundProgram2025KtId = Guid.Parse("bbbbbb02-1111-1111-1111-111111111111");
    public static readonly Guid RoundProgram2026Dot2CnttId = Guid.Parse("bbbbbb03-1111-1111-1111-111111111111");
    public static readonly Guid RoundProgram2026Dot3CnttId = Guid.Parse("bbbbbb04-1111-1111-1111-111111111111");
}
