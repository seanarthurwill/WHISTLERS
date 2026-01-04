using System.ComponentModel.DataAnnotations.Schema;

namespace GroupsService.Models
{
    [Table("official_groups")]
    public class Group
    {
        [Column("group_id")]
        public int GroupId { get; set; }
        
        [Column("sport_id")]
        public int SportId { get; set; }
        
        [Column("group_name")]
        public string GroupName { get; set; } = null!;
        
        [Column("description")]
        public string? Description { get; set; }
        
        [Column("created_by")]
        public int CreatedBy { get; set; }
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();
    }

    [Table("group_members")]
    public class GroupMember
    {
        [Column("group_member_id")]
        public int GroupMemberId { get; set; }
        
        [Column("group_id")]
        public int GroupId { get; set; }
        
        [Column("official_id")]
        public int OfficialId { get; set; }
        
        [Column("joined_at")]
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Group Group { get; set; } = null!;
    }
}
