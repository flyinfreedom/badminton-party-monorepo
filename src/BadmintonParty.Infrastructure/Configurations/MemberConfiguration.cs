using BadmintonParty.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BadmintonParty.Infrastructure.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<MemberEntity>
{
    public void Configure(EntityTypeBuilder<MemberEntity> builder)
    {
        builder.HasKey(e => e.MemberId);
        builder.HasIndex(e => e.LineUserId).IsUnique();
    }
}
