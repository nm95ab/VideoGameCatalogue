using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoGameCatalogue.Domain.Lookups;

namespace VideoGameCatalogue.Infrastructure.Persistence.Configurations;

public class PlatformLookupConfiguration : IEntityTypeConfiguration<PlatformLookup>
{
    public void Configure(EntityTypeBuilder<PlatformLookup> builder)
    {
        builder.ToTable("Platforms");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
        builder.HasIndex(x => x.Name).IsUnique();
    }
}
