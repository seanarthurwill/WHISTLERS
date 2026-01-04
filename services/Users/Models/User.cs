using System.ComponentModel.DataAnnotations.Schema;

namespace UsersService.Models
{
    [Table("users")]
    public class User
    {
        [Column("user_id")]
        public int UserId { get; set; }
        
        [Column("tenant_id")]
        public Guid TenantId { get; set; } = Guid.NewGuid();
        
        [Column("role_id")]
        public int? RoleId { get; set; }
        
        [Column("first_name")]
        public string FirstName { get; set; } = null!;
        
        [Column("last_name")]
        public string LastName { get; set; } = null!;
        
        [Column("email")]
        public string Email { get; set; } = null!;
        
        [Column("phone")]
        public string? Phone { get; set; }
        
        [Column("password_hash")]
        public string PasswordHash { get; set; } = null!;
        
        [Column("date_of_birth")]
        public DateTime? DateOfBirth { get; set; }
        
        [Column("user_type")]
        public string? UserType { get; set; } // Official, Assignor, Parent, Mentor
        
        [Column("is_active")]
        public bool IsActive { get; set; } = true;
        
        [Column("email_verified")]
        public bool EmailVerified { get; set; } = false;
        
        [Column("phone_verified")]
        public bool PhoneVerified { get; set; } = false;
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Column("last_login")]
        public DateTime? LastLogin { get; set; }

        [Column("reset_password_guid")]
        public Guid? ResetPasswordGuid { get; set; }

        // Navigation property
        public Role? Role { get; set; }
    }

    [Table("roles")]
    public class Role
    {
        [Column("role_id")]
        public int RoleId { get; set; }
        
        [Column("role_name")]
        public string RoleName { get; set; } = null!;
        
        [Column("description")]
        public string? Description { get; set; }
        
        [Column("is_system_role")]
        public bool IsSystemRole { get; set; } = false;
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
