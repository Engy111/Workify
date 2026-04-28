using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Workify_Full.Models
{
    public class DisputeMessage
    {
        [Key]
        public int DisputeMessageId { get; set; }
        public string Body { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // Foreign Key -> Dispute
        [Required]
        public int DisputeId { get; set; }
        [ForeignKey("DisputeId")]
        public Dispute? Dispute { get; set; }

        // Foreign Key -> ApplicationUser
        [Required]
        public string SenderId { get; set; } = string.Empty;
        [ForeignKey("SenderId")]
        public ApplicationUser? Sender { get; set; }
    }
}
