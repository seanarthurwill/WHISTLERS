using System.ComponentModel.DataAnnotations.Schema;

namespace OrganizationsService.Models
{
    [Table("official_organizations")]
    public class OfficialOrganization
    {
        [Column("official_organization_id")]
        public int OfficialOrganizationId { get; set; }
        
        [Column("official_id")]
        public int OfficialId { get; set; }
        
        [Column("organization_id")]
        public int OrganizationId { get; set; }
        
        [Column("joined_at")]
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public Organization? Organization { get; set; }
    }
}
