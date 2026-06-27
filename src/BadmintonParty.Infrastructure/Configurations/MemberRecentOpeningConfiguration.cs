using BadmintonParty.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BadmintonParty.Infrastructure.Configurations;

public class MemberRecentOpeningConfiguration : IEntityTypeConfiguration<MemberRecentOpeningEntity>
{
    public void Configure(EntityTypeBuilder<MemberRecentOpeningEntity> builder)
    {
        builder.HasKey(e => new { e.MemberId, e.CourtId });
        
        builder.HasOne<MemberEntity>()
            .WithMany()
            .HasForeignKey(e => e.MemberId);
            
        builder.HasOne<CourtEntity>()
            .WithMany()
            .HasForeignKey(e => e.CourtId);
    }
}
