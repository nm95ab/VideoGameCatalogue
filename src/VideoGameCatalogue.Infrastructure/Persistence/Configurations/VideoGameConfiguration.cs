using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoGameCatalogue.Domain.Games;
using VideoGameCatalogue.Domain.Games.ValueObjects;

namespace VideoGameCatalogue.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core relational mapping configuration for the <see cref="VideoGame"/> Aggregate Root.
/// <para>
/// Architectural Design Note:
/// Domain Value Objects (Title, Platform, Genre, ReleaseYear, Rating) are mapped as EF Core Complex Types
/// using <c>ComplexProperty</c> rather than legacy <c>HasConversion</c> Value Converters.
/// This registers the inner <c>.Value</c> as a first-class mapped column in EF Core's relational expression
/// tree, allowing LINQ queries like <c>g.Title.Value.Contains(search)</c> and <c>OrderBy(g => g.Title.Value)</c>.
/// <para>
/// Indexing Strategy:
/// Because EF Core 10 Fluent API does not yet support declaring indexes directly on properties inside
/// <c>ComplexProperty</c> mappings (scheduled for EF Core 11+), high-performance relational non-clustered indexes
/// for [Platform], [Genre], [Title], and composite [Platform, Genre, Title] are provisioned programmatically
/// via <see cref="DatabaseSeeder.MigrateSchemaAsync"/> upon application startup and schema migration.
/// </para>
/// </para>
/// </summary>
public class VideoGameConfiguration : IEntityTypeConfiguration<VideoGame>
{
    public void Configure(EntityTypeBuilder<VideoGame> builder)
    {
        builder.ToTable("VideoGames");

        builder.HasKey(x => x.Id);

        // Map GameTitle Value Object as a Complex Type to column [Title]
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
