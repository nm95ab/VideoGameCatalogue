using Microsoft.EntityFrameworkCore;
using VideoGameCatalogue.Domain.Games;
using VideoGameCatalogue.Domain.Lookups;
using VideoGameCatalogue.Infrastructure.Persistence.Configurations;

namespace VideoGameCatalogue.Infrastructure.Persistence;

public class VideoGameCatalogueDbContext(
    DbContextOptions<VideoGameCatalogueDbContext> options,
    IEntityTypeConfiguration<VideoGame>? videoGameConfiguration = null) : DbContext(options)
{
    public DbSet<VideoGame> VideoGames => Set<VideoGame>();
    public DbSet<PlatformLookup> Platforms => Set<PlatformLookup>();
    public DbSet<GenreLookup> Genres => Set<GenreLookup>();
    public DbSet<RatingLookup> Ratings => Set<RatingLookup>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new PlatformLookupConfiguration());
        modelBuilder.ApplyConfiguration(new GenreLookupConfiguration());
        modelBuilder.ApplyConfiguration(new RatingLookupConfiguration());

        var config = videoGameConfiguration ?? (Database.IsInMemory()
            ? (IEntityTypeConfiguration<VideoGame>)new InMemoryVideoGameConfiguration()
            : new SqlServerVideoGameConfiguration());

        modelBuilder.ApplyConfiguration(config);
    }
}
