using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class AuthorTypeConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.HasKey(e => e.AuthorId);
        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(Author.MaxNameLength);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(Author.MaxNameLength);
    }
}