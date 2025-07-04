using System.Reflection;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

/// <summary>
/// Application Db context.
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// Authors.
    /// </summary>
    public DbSet<Author> Authors { get; set; }

    /// <summary>
    /// Genres.
    /// </summary>
    public DbSet<Genre> Genres { get; set; }
    
    /// <summary>
    /// Books.
    /// </summary>
    public DbSet<Book> Books { get; set; }
    
    /// <summary>
    /// Initializes a new instance of <see cref="ApplicationDbContext"/>.
    /// </summary>
    /// <param name="options"><see cref="DbContextOptions"/></param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }
    
    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}