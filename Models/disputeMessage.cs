using Microsoft.AspNetCore.Identity;

namespace Workify_Full.Models
{
    public class DisputeMessage
    {
        public int Id { get; set; }

        public string Message { get; set; } = string.Empty;

        public DateTime SentAt { get; set; }

        public int DisputeId { get; set; }

        public string SenderId { get; set; } = string.Empty;
        public IdentityUser? Sender { get; set; }
    }
}