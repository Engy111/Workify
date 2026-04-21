using Microsoft.AspNetCore.Identity;

namespace Workify_Full.Models
{
    public class Wallet
    {
        public int Id { get; set; }

        public decimal AvailableBalance { get; set; }
        public decimal EscrowBalance { get; set; }

        public string UserId { get; set; } = string.Empty;

        public IdentityUser? User { get; set; }
    }
}