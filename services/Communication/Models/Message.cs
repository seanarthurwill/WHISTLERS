using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunicationService.Models
{
    [Table("messages")]
    public class Message
    {
        [Key]
        [Column("message_id")]
        public int MessageId { get; set; }

        [Column("sender_user_id")]
        public int SenderUserId { get; set; }

        [Column("recipient_user_id")]
        public int? RecipientUserId { get; set; }

        [Column("recipient_group_id")]
        public int? RecipientGroupId { get; set; }

        [Required]
        [Column("subject")]
        [MaxLength(500)]
        public string Subject { get; set; } = null!;

        [Required]
        [Column("message_body")]
        public string MessageBody { get; set; } = null!;

        [Column("is_read")]
        public bool IsRead { get; set; } = false;

        [Column("sent_at")]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        [Column("read_at")]
        public DateTime? ReadAt { get; set; }
    }
}
