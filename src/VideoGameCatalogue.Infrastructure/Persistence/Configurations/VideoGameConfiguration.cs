using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoGameCatalogue.Domain.Games;
using VideoGameCatalogue.Domain.Games.ValueObjects;

namespace VideoGameCatalogue.Infrastructure.Persistence.Configurations;

public class VideoGameConfiguration : IEntityTypeConfiguration<VideoGame>
{
    public void Configure(EntityTypeBuilder<VideoGame> builder)
    {
        builder.ToTable("VideoGames");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasConversion(
                title => title.Value,
                value => GameTitle.Create(value).Value)
            .HasMaxLength(GameTitle.MaxLength)
            .IsRequired();

        builder.Property(x => x.Platform)
            .HasConversion(
                platform => platform.Value,
                value => Platform.Create(value).Value)
            .HasMaxLength(Platform.MaxLength)
            .IsRequired();

        builder.Property(x => x.Genre)
            .HasConversion(
                genre => genre.Value,
                value => Genre.Create(value).Value)
            .HasMaxLength(Genre.MaxLength)
            .IsRequired();

        builder.Property(x => x.ReleaseYear)
            .HasConversion(
                year => year.Value,
                value => ReleaseYear.Create(value).Value)
            .IsRequired();

        builder.Property(x => x.Rating)
            .HasConversion(
                rating => rating.Value,
                value => Rating.Create(value).Value)
            .HasMaxLength(Rating.MaxLength)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc);

        builder.HasIndex(x => x.Title);
        builder.HasIndex(x => x.Platform);
        builder.HasIndex(x => x.Genre);
    }
}
