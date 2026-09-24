using System.Reflection;
using Microsoft.EntityFrameworkCore;
using VideoGameCatalogue.Domain.Games;
using VideoGameCatalogue.Domain.Lookups;

namespace VideoGameCatalogue.Infrastructure.Persistence;

public class VideoGameCatalogueDbContext(DbContextOptions<VideoGameCatalogueDbContext> options) : DbContext(options)
{
    public DbSet<VideoGame> VideoGames => Set<VideoGame>();
    public DbSet<PlatformLookup> Platforms => Set<PlatformLookup>();
    public DbSet<GenreLookup> Genres => Set<GenreLookup>();
    public DbSet<RatingLookup> Ratings => Set<RatingLookup>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
