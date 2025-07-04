using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

/// <summary>
/// Genre type configuration.
/// </summary>
public class GenreTypeConfiguration : IEntityTypeConfiguration<Genre>
{
        
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Genre> builder)
    {
        builder.HasKey(e => e.GenreName);
        builder.Property(e => e.GenreName).IsRequired().HasMaxLength(Genre.MaxGenreNameLength);
    }
}