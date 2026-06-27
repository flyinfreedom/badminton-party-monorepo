using BadmintonParty.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace BadmintonParty.Infrastructure.Contexts;

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
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BadmintonContext).Assembly);
    }
}
