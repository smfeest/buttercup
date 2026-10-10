using Microsoft.EntityFrameworkCore;

namespace Buttercup.EntityModel;

/// <summary>
/// Represents a session with the application database.
/// </summary>
public sealed class AppDbContext : DbContext
{
    private const string CaseInsensitiveCollationName = "und-ci";
    private const string OrdinalCollationName = "C";

    /// <summary>
    /// Initializes a new instance of the <see cref="AppDbContext" /> class.
    /// </summary>
    public AppDbContext()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppDbContext" /> class.
    /// </summary>
    /// <param name="options">The options for this context.</param>
    public AppDbContext(DbContextOptions options)
        : base(options)
    { }

    /// <summary>
    /// Gets the set of all comments.
    /// </summary>
    public DbSet<Comment> Comments => this.Set<Comment>();

    /// <summary>
    /// Gets the set of all comment audit entries.
    /// </summary>
    public DbSet<CommentAudit> CommentAudits => this.Set<CommentAudit>();

    /// <summary>
    /// Gets the set of all comment revisions.
    /// </summary>
    public DbSet<CommentRevision> CommentRevisions => this.Set<CommentRevision>();

    /// <summary>
    /// Gets the set of all password reset tokens.
    /// </summary>
    public DbSet<PasswordResetToken> PasswordResetTokens => this.Set<PasswordResetToken>();

    /// <summary>
    /// Gets the set of all recipes.
    /// </summary>
    public DbSet<Recipe> Recipes => this.Set<Recipe>();

    /// <summary>
    /// Gets the set of all recipe audit entries.
    /// </summary>
    public DbSet<RecipeAudit> RecipeAudits => this.Set<RecipeAudit>();

    /// <summary>
    /// Gets the set of all recipe revisions.
    /// </summary>
    public DbSet<RecipeRevision> RecipeRevisions => this.Set<RecipeRevision>();

    /// <summary>
    /// Gets the set of all users.
    /// </summary>
    public DbSet<User> Users => this.Set<User>();

    /// <summary>
    /// Gets the set of all user audit entries.
    /// </summary>
    public DbSet<UserAuditEntry> UserAuditEntries => this.Set<UserAuditEntry>();

    /// <inheritdoc/>
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.Properties<string>().UseCollation(CaseInsensitiveCollationName);

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasCollation(
            CaseInsensitiveCollationName,
            locale: "und-u-ks-level2",
            provider: "icu",
            deterministic: false);

        modelBuilder
            .Entity<CommentAudit>()
            .Property(e => e.Action)
            .HasConversion<CommentActionToStringConverter>()
            .HasMaxLength(10)
            .UseCollation(OrdinalCollationName);
        modelBuilder
            .Entity<PasswordResetToken>()
            .Property(e => e.Token)
            .UseCollation(OrdinalCollationName);
        modelBuilder
            .Entity<RecipeAudit>()
            .Property(e => e.Action)
            .HasConversion<RecipeActionToStringConverter>()
            .HasMaxLength(10)
            .UseCollation(OrdinalCollationName);
        modelBuilder
            .Entity<User>()
            .Property(e => e.HashedPassword)
            .UseCollation(OrdinalCollationName);
        modelBuilder
            .Entity<User>()
            .Property(e => e.SecurityStamp)
            .UseCollation(OrdinalCollationName);
        modelBuilder
            .Entity<User>()
            .Property(e => e.TimeZone)
            .UseCollation(OrdinalCollationName);
        modelBuilder
            .Entity<UserAuditEntry>()
            .Property(e => e.Operation)
            .HasConversion<UserAuditOperationToStringConverter>()
            .HasMaxLength(30)
            .UseCollation(OrdinalCollationName);
        modelBuilder
            .Entity<UserAuditEntry>()
            .Property(e => e.Failure)
            .HasConversion<UserAuditFailureToStringConverter>()
            .HasMaxLength(30)
            .UseCollation(OrdinalCollationName);
    }
}
