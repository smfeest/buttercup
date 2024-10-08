using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Buttercup.EntityModel;

/// <summary>
/// Represents a session with the application database.
/// </summary>
public sealed class AppDbContext : DbContext
{
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
    /// Gets the set of all comment revisions.
    /// </summary>
    public DbSet<CommentRevision> CommentRevisions => this.Set<CommentRevision>();

    /// <summary>
    /// Gets the set of all security events.
    /// </summary>
    public DbSet<SecurityEvent> SecurityEvents => this.Set<SecurityEvent>();

    /// <summary>
    /// Gets the set of all password reset tokens.
    /// </summary>
    public DbSet<PasswordResetToken> PasswordResetTokens => this.Set<PasswordResetToken>();

    /// <summary>
    /// Gets the set of all recipes.
    /// </summary>
    public DbSet<Recipe> Recipes => this.Set<Recipe>();

    /// <summary>
    /// Gets the set of all recipe revisions.
    /// </summary>
    public DbSet<RecipeRevision> RecipeRevisions => this.Set<RecipeRevision>();

    /// <summary>
    /// Gets the set of all users.
    /// </summary>
    public DbSet<User> Users => this.Set<User>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder
            .Entity<SecurityEvent>()
            .Property(u => u.IpAddress)
            .HasConversion<IPAddressToBytesConverter>();
}
