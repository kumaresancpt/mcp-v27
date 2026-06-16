using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<AuthSession> AuthSessions => Set<AuthSession>();
    public DbSet<AccountLockout> AccountLockouts => Set<AccountLockout>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(item => item.Id);

            entity.Property(item => item.Id).HasColumnName("id");
            entity.Property(item => item.Name).HasColumnName("name").HasMaxLength(64).IsRequired();
            entity.Property(item => item.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone").HasDefaultValueSql("now()");
            entity.Property(item => item.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone").HasDefaultValueSql("now()");

            entity.HasIndex(item => item.Name).IsUnique().HasDatabaseName("ux_roles_name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users", table =>
            {
                table.HasCheckConstraint("ck_users_email_or_employee_id", "email IS NOT NULL OR employee_id IS NOT NULL");
            });

            entity.HasKey(item => item.Id);

            entity.Property(item => item.Id).HasColumnName("id");
            entity.Property(item => item.Email).HasColumnName("email").HasMaxLength(320);
            entity.Property(item => item.EmployeeId).HasColumnName("employee_id").HasMaxLength(64);
            entity.Property(item => item.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
            entity.Property(item => item.RoleId).HasColumnName("role_id");
            entity.Property(item => item.IsActive).HasColumnName("is_active").HasDefaultValue(true).IsRequired();
            entity.Property(item => item.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone").HasDefaultValueSql("now()");
            entity.Property(item => item.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone").HasDefaultValueSql("now()");

            entity.HasIndex(item => item.Email).IsUnique().HasDatabaseName("ux_users_email").HasFilter("email IS NOT NULL");
            entity.HasIndex(item => item.EmployeeId).IsUnique().HasDatabaseName("ux_users_employee_id").HasFilter("employee_id IS NOT NULL");
            entity.HasIndex(item => item.RoleId).HasDatabaseName("ix_users_role_id");

            entity.HasOne(item => item.Role)
                .WithMany(item => item.Users)
                .HasForeignKey(item => item.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuthSession>(entity =>
        {
            entity.ToTable("auth_sessions");
            entity.HasKey(item => item.Id);

            entity.Property(item => item.Id).HasColumnName("id");
            entity.Property(item => item.UserId).HasColumnName("user_id");
            entity.Property(item => item.SessionTokenId).HasColumnName("session_token_id").HasMaxLength(64).IsRequired();
            entity.Property(item => item.IssuedAtUtc).HasColumnName("issued_at_utc").HasColumnType("timestamp with time zone").HasDefaultValueSql("now()");
            entity.Property(item => item.ExpiresAtUtc).HasColumnName("expires_at_utc").HasColumnType("timestamp with time zone").IsRequired();
            entity.Property(item => item.RevokedAtUtc).HasColumnName("revoked_at_utc").HasColumnType("timestamp with time zone");
            entity.Property(item => item.KeepMeLoggedIn).HasColumnName("keep_me_logged_in").HasDefaultValue(false).IsRequired();
            entity.Property(item => item.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone").HasDefaultValueSql("now()");

            entity.HasIndex(item => item.SessionTokenId).IsUnique().HasDatabaseName("ux_auth_sessions_session_token_id");
            entity.HasIndex(item => item.UserId).HasDatabaseName("ix_auth_sessions_user_id");
            entity.HasIndex(item => item.ExpiresAtUtc).HasDatabaseName("ix_auth_sessions_expires_at_utc");

            entity.HasOne(item => item.User)
                .WithMany(item => item.AuthSessions)
                .HasForeignKey(item => item.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AccountLockout>(entity =>
        {
            entity.ToTable("account_lockouts");
            entity.HasKey(item => item.Id);

            entity.Property(item => item.Id).HasColumnName("id");
            entity.Property(item => item.UserId).HasColumnName("user_id");
            entity.Property(item => item.FailedAttemptCount).HasColumnName("failed_attempt_count").HasDefaultValue(0).IsRequired();
            entity.Property(item => item.LockedUntilUtc).HasColumnName("locked_until_utc").HasColumnType("timestamp with time zone");
            entity.Property(item => item.LastFailedAtUtc).HasColumnName("last_failed_at_utc").HasColumnType("timestamp with time zone");
            entity.Property(item => item.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone").HasDefaultValueSql("now()");

            entity.HasIndex(item => item.UserId).IsUnique().HasDatabaseName("ux_account_lockouts_user_id");
            entity.HasIndex(item => item.LockedUntilUtc).HasDatabaseName("ix_account_lockouts_locked_until_utc");

            entity.HasOne(item => item.User)
                .WithOne(item => item.AccountLockout)
                .HasForeignKey<AccountLockout>(item => item.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.ToTable("password_reset_tokens");
            entity.HasKey(item => item.Id);

            entity.Property(item => item.Id).HasColumnName("id");
            entity.Property(item => item.UserId).HasColumnName("user_id");
            entity.Property(item => item.TokenHash).HasColumnName("token_hash").HasMaxLength(255).IsRequired();
            entity.Property(item => item.ExpiresAtUtc).HasColumnName("expires_at_utc").HasColumnType("timestamp with time zone").IsRequired();
            entity.Property(item => item.UsedAtUtc).HasColumnName("used_at_utc").HasColumnType("timestamp with time zone");
            entity.Property(item => item.IsUsed).HasColumnName("is_used").HasDefaultValue(false).IsRequired();
            entity.Property(item => item.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone").HasDefaultValueSql("now()");

            entity.HasIndex(item => item.UserId).HasDatabaseName("ix_password_reset_tokens_user_id");
            entity.HasIndex(item => item.ExpiresAtUtc).HasDatabaseName("ix_password_reset_tokens_expires_at_utc");

            entity.HasOne(item => item.User)
                .WithMany(item => item.PasswordResetTokens)
                .HasForeignKey(item => item.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_logs");
            entity.HasKey(item => item.Id);

            entity.Property(item => item.Id).HasColumnName("id");
            entity.Property(item => item.Actor).HasColumnName("actor").HasMaxLength(320).IsRequired();
            entity.Property(item => item.EventType).HasColumnName("event_type").HasMaxLength(128).IsRequired();
            entity.Property(item => item.EventTimestampUtc).HasColumnName("event_timestamp_utc").HasColumnType("timestamp with time zone").HasDefaultValueSql("now()");
            entity.Property(item => item.MetadataJson).HasColumnName("metadata_json").HasColumnType("jsonb").IsRequired();
            entity.Property(item => item.PreviousHash).HasColumnName("previous_hash").HasMaxLength(128).IsRequired();
            entity.Property(item => item.Hash).HasColumnName("hash").HasMaxLength(128).IsRequired();

            entity.HasIndex(item => item.Hash).IsUnique().HasDatabaseName("ux_audit_logs_hash");
            entity.HasIndex(item => item.EventTimestampUtc).HasDatabaseName("ix_audit_logs_event_timestamp_utc");
            entity.HasIndex(item => item.EventType).HasDatabaseName("ix_audit_logs_event_type");
        });
    }
}