using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Contracts;
namespace Workify_Full.Models
{
    public class Wallet
    {
        [Key]
        public required int WalletId { get; set; }
        public decimal AvailableBalance { get; set; } = 0m;
        public decimal EscrowBalance { get; set; } = 0m;
        public string Currency { get; set; } = "EGP";

        // Foreign Key -> ApplicationUser (one-to-one)
        [Required]
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        // Navigation Properties
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
