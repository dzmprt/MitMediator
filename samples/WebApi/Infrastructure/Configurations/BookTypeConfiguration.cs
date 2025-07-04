using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

/// <summary>
/// Book type configuration.
/// </summary>
public class BookTypeConfiguration : IEntityTypeConfiguration<Book>
{
        
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.HasKey(e => e.BookId);
        builder.Property(e => e.Title).IsRequired().HasMaxLength(Book.MaxTitleLength);
        builder.Navigation(e => e.Author).IsRequired().AutoInclude();
        builder.Navigation(e => e.Genre).IsRequired().AutoInclude();
    }
}