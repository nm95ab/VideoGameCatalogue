using System.Reflection;
using Microsoft.EntityFrameworkCore;
using VideoGameCatalogue.Domain.Games;

namespace VideoGameCatalogue.Infrastructure.Persistence;

public class VideoGameCatalogueDbContext(DbContextOptions<VideoGameCatalogueDbContext> options) : DbContext(options)
{
    public DbSet<VideoGame> VideoGames => Set<VideoGame>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
