using BadmintonParty.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BadmintonParty.Infrastructure.Configurations;

public class GroupConfiguration : IEntityTypeConfiguration<GroupEntity>
{
    public void Configure(EntityTypeBuilder<GroupEntity> builder)
    {
        builder.HasKey(e => e.GroupId);
        
        builder.HasOne<CourtEntity>()
            .WithMany()
            .HasForeignKey(e => e.CourtId);
        
        builder.HasOne(e => e.Creator)
            .WithMany()
            .HasForeignKey(e => e.MemberId);
    }
}
