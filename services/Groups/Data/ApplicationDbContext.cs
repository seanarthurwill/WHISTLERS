using Microsoft.EntityFrameworkCore;
using GroupsService.Models;

namespace GroupsService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Group> Groups { get; set; } = null!;
        public DbSet<GroupMember> GroupMembers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Group configuration
            modelBuilder.Entity<Group>(entity =>
            {
                entity.HasKey(e => e.GroupId);
                entity.ToTable("official_groups");
                
                entity.Property(e => e.GroupName).IsRequired().HasMaxLength(200);
            });

            // GroupMember configuration
            modelBuilder.Entity<GroupMember>(entity =>
            {
                entity.HasKey(e => e.GroupMemberId);
                entity.ToTable("group_members");

                entity.HasOne(e => e.Group)
                    .WithMany(g => g.GroupMembers)
                    .HasForeignKey(e => e.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.GroupId, e.OfficialId }).IsUnique();
            });
        }
    }
}
