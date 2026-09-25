using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoGameCatalogue.Domain.Games;
using VideoGameCatalogue.Domain.Games.ValueObjects;

namespace VideoGameCatalogue.Infrastructure.Persistence.Configurations;

/// <summary>
/// Relational Entity Framework Core mapping configuration for Microsoft SQL Server.
/// Maps Domain Value Objects as Complex Types (<c>ComplexProperty</c>) with native column mappings.
/// </summary>
public class SqlServerVideoGameConfiguration : IEntityTypeConfiguration<VideoGame>
{
    public void Configure(EntityTypeBuilder<VideoGame> builder)
    {
        builder.ToTable("VideoGames");
        builder.HasKey(x => x.Id);

        builder.ComplexProperty(x => x.Title, b =>
        {
            b.Property(p => p.Value)
                .HasColumnName("Title")
                .HasMaxLength(GameTitle.MaxLength)
                .IsRequired();
        });

        builder.ComplexProperty(x => x.Platform, b =>
        {
            b.Property(p => p.Value)
                .HasColumnName("Platform")
                .HasMaxLength(Platform.MaxLength)
                .IsRequired();
        });

        builder.ComplexProperty(x => x.Genre, b =>
        {
            b.Property(p => p.Value)
                .HasColumnName("Genre")
                .HasMaxLength(Genre.MaxLength)
                .IsRequired();
        });

        builder.ComplexProperty(x => x.ReleaseYear, b =>
        {
            b.Property(p => p.Value)
                .HasColumnName("ReleaseYear")
                .IsRequired();
        });

        builder.ComplexProperty(x => x.Rating, b =>
        {
            b.Property(p => p.Value)
                .HasColumnName("Rating")
                .HasMaxLength(Rating.MaxLength)
                .IsRequired();
        });

        builder.Property(x => x.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.ImageId)
            .HasMaxLength(100);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc);
    }
}
