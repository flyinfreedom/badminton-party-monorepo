using BadmintonParty.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BadmintonParty.Infrastructure.Configurations;

public class GroupMemberConfiguration : IEntityTypeConfiguration<GroupMemberEntity>
{
    public void Configure(EntityTypeBuilder<GroupMemberEntity> builder)
    {
        builder.HasKey(e => e.GroupMemberId);
        
        builder.HasOne(e => e.Group)
            .WithMany(g => g.Members)
            .HasForeignKey(e => e.GroupId);
        
        builder.HasOne(e => e.Member)
            .WithMany()
            .HasForeignKey(e => e.MemberId);
    }
}
