using System.ComponentModel.DataAnnotations.Schema;

namespace Whistl3rApi.Models
{
    [Table("assignors")]
    public class Assignor
    {
        [Column("assignor_id")]
        public int AssignorId { get; set; }
        
        [Column("user_id")]
        public int UserId { get; set; }
        
        [Column("is_super_admin")]
        public bool IsSuperAdmin { get; set; } = false;

        // Navigation properties
        public ICollection<AssignorOrganization> AssignorOrganizations { get; set; } = new List<AssignorOrganization>();
    }

    [Table("assignor_organizations")]
    public class AssignorOrganization
    {
        [Column("assignor_organization_id")]
        public int AssignorOrganizationId { get; set; }
        
        [Column("assignor_id")]
        public int AssignorId { get; set; }
        
        [Column("organization_id")]
        public int OrganizationId { get; set; }
        
        [Column("role_level")]
        public string? RoleLevel { get; set; } // Admin, Manager, Viewer
        
        [Column("assigned_at")]
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        
        [Column("assigned_by")]
        public int? AssignedBy { get; set; }
        
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public Assignor Assignor { get; set; } = null!;
    }
}
