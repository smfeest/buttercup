using Microsoft.EntityFrameworkCore;

namespace Buttercup.EntityModel;

/// <summary>
/// Represents a session with the application database.
/// </summary>
public class AppDbContext : DbContext
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
    /// Gets the set of all authentication events.
    /// </summary>
    public DbSet<AuthenticationEvent> AuthenticationEvents => this.Set<AuthenticationEvent>();

    /// <summary>
    /// Gets the set of all password reset tokens.
    /// </summary>
    public DbSet<PasswordResetToken> PasswordResetTokens => this.Set<PasswordResetToken>();

    /// <summary>
    /// Gets the set of all recipes.
    /// </summary>
    public DbSet<Recipe> Recipes => this.Set<Recipe>();

    /// <summary>
    /// Gets the set of all users.
    /// </summary>
    public DbSet<User> Users => this.Set<User>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder
            .Entity<User>()
            .HasAlternateKey(u => u.Email);
}
