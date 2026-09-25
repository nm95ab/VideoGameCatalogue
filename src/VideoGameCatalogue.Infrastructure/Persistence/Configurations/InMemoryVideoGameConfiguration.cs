using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoGameCatalogue.Domain.Games;
using VideoGameCatalogue.Domain.Games.ValueObjects;

namespace VideoGameCatalogue.Infrastructure.Persistence.Configurations;

/// <summary>
/// In-Memory Entity Framework Core mapping configuration for Microsoft.EntityFrameworkCore.InMemory.
/// Maps Domain Value Objects via Value Converters (<c>HasConversion</c>) to avoid the known EF Core InMemory
/// complex property dictionary bug while fully supporting in-memory LINQ expression execution.
/// </summary>
public class InMemoryVideoGameConfiguration : IEntityTypeConfiguration<VideoGame>
{
    public void Configure(EntityTypeBuilder<VideoGame> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasConversion(t => t.Value, v => GameTitle.Create(v).Value)
            .HasMaxLength(GameTitle.MaxLength)
            .IsRequired();

        builder.Property(x => x.Platform)
            .HasConversion(p => p.Value, v => Platform.Create(v).Value)
            .HasMaxLength(Platform.MaxLength)
            .IsRequired();

        builder.Property(x => x.Genre)
            .HasConversion(g => g.Value, v => Genre.Create(v).Value)
            .HasMaxLength(Genre.MaxLength)
            .IsRequired();

        builder.Property(x => x.ReleaseYear)
            .HasConversion(y => y.Value, v => ReleaseYear.Create(v).Value)
            .IsRequired();

        builder.Property(x => x.Rating)
            .HasConversion(r => r.Value, v => Rating.Create(v).Value)
            .HasMaxLength(Rating.MaxLength)
            .IsRequired();

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
