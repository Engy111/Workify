using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Workify_Full.Data;
using Workify_Full.Models;
using Workify_Full.Models.Enum;

namespace Workify_Full.Controllers
{
    public class WalletController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WalletController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Wallet/Overview/5
        [HttpGet]
        public async Task<IActionResult> Overview(int id)
        {
            var wallet = await _context.Wallets.FindAsync(id);

            if (wallet == null)
            {
                return NotFound();
            }

            var transactions = await _context.Transactions
                .Where(t => t.WalletId == id)
                .ToListAsync();

            ViewBag.Transactions = transactions;

            return View(wallet);
        }

        // POST: /Wallet/Deposit
        [HttpPost]
        public async Task<IActionResult> Deposit(int walletId, decimal amount)
        {
            if (amount <= 0)
            {
                ModelState.AddModelError("", "Deposit amount must be greater than zero.");
                return RedirectToAction("Overview", new { id = walletId });
            }

            var wallet = await _context.Wallets.FindAsync(walletId);

            if (wallet == null)
            {
                return NotFound();
            }

            wallet.AvailableBalance += amount;

            var transaction = new Transaction
            {
                WalletId = wallet.WalletId,
                Amount = amount,
                Type = TransactionType.Deposit
            };

            SetPropertyIfExists(transaction, "CreatedAt", DateTime.Now);

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return RedirectToAction("Overview", new { id = wallet.WalletId });
        }

        // POST: /Wallet/Withdraw
        [HttpPost]
        public async Task<IActionResult> Withdraw(int walletId, decimal amount)
        {
            if (amount <= 0)
            {
                ModelState.AddModelError("", "Withdrawal amount must be greater than zero.");
                return RedirectToAction("Overview", new { id = walletId });
            }

            var wallet = await _context.Wallets.FindAsync(walletId);

            if (wallet == null)
            {
                return NotFound();
            }

            if (wallet.AvailableBalance < amount)
            {
                ModelState.AddModelError("", "Insufficient available balance.");
                return RedirectToAction("Overview", new { id = wallet.WalletId });
            }

            wallet.AvailableBalance -= amount;

            var transaction = new Transaction
            {
                WalletId = wallet.WalletId,
                Amount = amount,
                Type = TransactionType.Withdrawal
            };

            SetPropertyIfExists(transaction, "CreatedAt", DateTime.Now);

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return RedirectToAction("Overview", new { id = wallet.WalletId });
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