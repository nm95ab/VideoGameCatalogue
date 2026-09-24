using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoGameCatalogue.Domain.Lookups;

namespace VideoGameCatalogue.Infrastructure.Persistence.Configurations;

public class GenreLookupConfiguration : IEntityTypeConfiguration<GenreLookup>
{
    public void Configure(EntityTypeBuilder<GenreLookup> builder)
    {
        builder.ToTable("Genres");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
        builder.HasIndex(x => x.Name).IsUnique();
    }
}
