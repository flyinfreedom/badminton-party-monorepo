using BadmintonParty.Liff.Web.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace BadmintonParty.Liff.Web.Api.Contexts;

public class BadmintonContext : DbContext
{
    public BadmintonContext(DbContextOptions<BadmintonContext> options) : base(options)
    {
    }

    public DbSet<MemberEntity> Members { get; set; }
    public DbSet<CourtEntity> Courts { get; set; }
    public DbSet<GroupEntity> Groups { get; set; }
    public DbSet<GroupMemberEntity> GroupMembers { get; set; }
    public DbSet<MemberRecentOpeningEntity> MemberRecentOpenings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MemberEntity>(entity =>
        {
            entity.HasKey(e => e.MemberId);
            entity.HasIndex(e => e.LineUserId).IsUnique();
        });

        modelBuilder.Entity<CourtEntity>(entity =>
        {
            entity.HasKey(e => e.CourtId);
        });

        modelBuilder.Entity<GroupEntity>(entity =>
        {
            entity.HasKey(e => e.GroupId);
            entity.HasOne<CourtEntity>()
                .WithMany()
                .HasForeignKey(e => e.CourtId);
            
            entity.HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.MemberId);
        });

        modelBuilder.Entity<GroupMemberEntity>(entity =>
        {
            entity.HasKey(e => new { e.GroupId, e.MemberId });
            
            entity.HasOne(e => e.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(e => e.GroupId);
            
            entity.HasOne(e => e.Member)
                .WithMany()
                .HasForeignKey(e => e.MemberId);
        });

        modelBuilder.Entity<MemberRecentOpeningEntity>(entity =>
        {
            entity.HasKey(e => new { e.MemberId, e.CourtId });
            
            entity.HasOne<MemberEntity>()
                .WithMany()
                .HasForeignKey(e => e.MemberId);
                
            entity.HasOne<CourtEntity>()
                .WithMany()
                .HasForeignKey(e => e.CourtId);
        });
    }
}

