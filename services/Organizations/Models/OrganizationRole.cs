using System.ComponentModel.DataAnnotations.Schema;

namespace OrganizationsService.Models
{
    [Table("organization_roles")]
    public class OrganizationRole
    {
        [Column("organization_role_id")]
        public int OrganizationRoleId { get; set; }
        
        [Column("organization_id")]
        public int OrganizationId { get; set; }
        
        [Column("role_id")]
        public int RoleId { get; set; }
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public Organization? Organization { get; set; }
    }
}
