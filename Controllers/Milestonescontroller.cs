using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Workify_Full.Data;
using Workify_Full.Models;
using Workify_Full.Models.Enum;

namespace Workify_Full.Controllers
{
    public class MilestonesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MilestonesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: /Milestones/Submit/5
        [HttpPost]
        public async Task<IActionResult> Submit(int id)
        {
            var milestone = await _context.Milestones.FindAsync(id);

            if (milestone == null)
            {
                return NotFound();
            }

            milestone.Status = MilestoneStatus.Submitted;

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Contracts", new { id = milestone.ContractId });
        }

        // POST: /Milestones/Approve/5
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var milestone = await _context.Milestones.FindAsync(id);

            if (milestone == null)
            {
                return NotFound();
            }

            await _context.Entry(milestone)
                .Reference(m => m.Contract)
                .LoadAsync();

            milestone.Status = MilestoneStatus.Released;

            var contract = milestone.Contract;

            if (contract != null)
            {
                var clientWallet = await _context.Wallets
                    .FirstOrDefaultAsync(w => w.UserId == contract.ClientId);

                var freelancerWallet = await _context.Wallets
                    .FirstOrDefaultAsync(w => w.UserId == contract.FreelancerId);

                if (clientWallet != null && freelancerWallet != null)
                {
                    clientWallet.EscrowBalance -= milestone.Amount;
                    freelancerWallet.AvailableBalance += milestone.Amount;

                    var clientTransaction = new Transaction
                    {
                        WalletId = clientWallet.WalletId,
                        Amount = milestone.Amount,
                        Type = TransactionType.EscrowRelease
                    };

                    var freelancerTransaction = new Transaction
                    {
                        WalletId = freelancerWallet.WalletId,
                        Amount = milestone.Amount,
                        Type = TransactionType.EscrowRelease
                    };

                    SetPropertyIfExists(clientTransaction, "CreatedAt", DateTime.Now);
                    SetPropertyIfExists(freelancerTransaction, "CreatedAt", DateTime.Now);

                    _context.Transactions.Add(clientTransaction);
                    _context.Transactions.Add(freelancerTransaction);
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Contracts", new { id = milestone.ContractId });
        }

        // POST: /Milestones/Reject/5
        [HttpPost]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            var milestone = await _context.Milestones.FindAsync(id);

            if (milestone == null)
            {
                return NotFound();
            }

            milestone.Status = MilestoneStatus.InProgress;

            SetPropertyIfExists(milestone, "RejectionReason", reason);
            SetPropertyIfExists(milestone, "RejectReason", reason);
            SetPropertyIfExists(milestone, "Note", reason);

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Contracts", new { id = milestone.ContractId });
        }

        private void SetPropertyIfExists(object obj, string propertyName, object value)
        {
            var property = obj.GetType().GetProperty(propertyName);

            if (property != null && property.CanWrite)
            {
                property.SetValue(obj, value);
            }
        }
    }
}