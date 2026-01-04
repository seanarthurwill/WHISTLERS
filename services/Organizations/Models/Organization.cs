using System.ComponentModel.DataAnnotations.Schema;

namespace OrganizationsService.Models
{
    [Table("organizations")]
    public class Organization
    {
        [Column("organization_id")]
        public int OrganizationId { get; set; }
        
        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.NewGuid();
        
        [Column("organization_name")]
        public string OrganizationName { get; set; } = null!;
        
        [Column("organization_type")]
        public string? OrganizationType { get; set; }
        
        [Column("website")]
        public string? Website { get; set; }
        
        [Column("contact_email")]
        public string? ContactEmail { get; set; }
        
        [Column("contact_phone")]
        public string? ContactPhone { get; set; }
        
        [Column("address_line1")]
        public string? AddressLine1 { get; set; }
        
        [Column("address_line2")]
        public string? AddressLine2 { get; set; }
        
        [Column("city")]
        public string? City { get; set; }
        
        [Column("state_province")]
        public string? StateProvince { get; set; }
        
        [Column("postal_code")]
        public string? PostalCode { get; set; }
        
        [Column("country")]
        public string? Country { get; set; }
        
        [Column("is_active")]
        public bool IsActive { get; set; } = true;
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
