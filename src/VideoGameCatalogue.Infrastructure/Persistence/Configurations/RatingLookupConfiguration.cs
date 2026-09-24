using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoGameCatalogue.Domain.Lookups;

namespace VideoGameCatalogue.Infrastructure.Persistence.Configurations;

public class RatingLookupConfiguration : IEntityTypeConfiguration<RatingLookup>
{
    public void Configure(EntityTypeBuilder<RatingLookup> builder)
    {
        builder.ToTable("Ratings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
        builder.HasIndex(x => x.Name).IsUnique();
    }
}
