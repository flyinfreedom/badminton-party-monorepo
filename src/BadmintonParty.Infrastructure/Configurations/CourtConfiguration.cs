using BadmintonParty.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BadmintonParty.Infrastructure.Configurations;

public class CourtConfiguration : IEntityTypeConfiguration<CourtEntity>
{
    public void Configure(EntityTypeBuilder<CourtEntity> builder)
    {
        builder.HasKey(e => e.CourtId);
    }
}
