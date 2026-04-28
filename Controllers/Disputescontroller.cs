using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Workify_Full.Data;
using Workify_Full.Models;
using Workify_Full.Models.Enum;

namespace Workify_Full.Controllers
{
    public class DisputesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DisputesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: /Disputes/File
        [HttpPost]
        public async Task<IActionResult> File(int contractId, string reason)
        {
            var contract = await _context.Contracts.FindAsync(contractId);

            if (contract == null)
            {
                return NotFound();
            }

            var dispute = new Dispute
            {
                ContractId = contractId,
                Reason = reason,
                Status = DisputeStatus.Open
            };

            SetPropertyIfExists(dispute, "CreatedAt", DateTime.Now);

            contract.Status = ContractStatus.Disputed;

            var activeMilestone = await _context.Milestones
                .FirstOrDefaultAsync(m => m.ContractId == contractId && m.Status != MilestoneStatus.Released);

            if (activeMilestone != null)
            {
                activeMilestone.Status = MilestoneStatus.Disputed;
            }

            _context.Disputes.Add(dispute);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = contractId });
        }

        // GET: /Disputes/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var dispute = await _context.Disputes
                .Include(d => d.Contract)
                .FirstOrDefaultAsync(d => d.ContractId == id);

            if (dispute == null)
            {
                return NotFound();
            }

            return View(dispute);
        }

        // POST: /Disputes/SendMessage
        [HttpPost]
        public async Task<IActionResult> SendMessage(int disputeId, string message)
        {
            var dispute = await _context.Disputes
                .FirstOrDefaultAsync(d => d.ContractId == disputeId);

            if (dispute == null)
            {
                return NotFound();
            }

            var disputeMessage = new DisputeMessage
            {
                DisputeId = disputeId
            };

            SetPropertyIfExists(disputeMessage, "Message", message);
            SetPropertyIfExists(disputeMessage, "MessageText", message);
            SetPropertyIfExists(disputeMessage, "Content", message);
            SetPropertyIfExists(disputeMessage, "Text", message);
            SetPropertyIfExists(disputeMessage, "CreatedAt", DateTime.Now);
            SetPropertyIfExists(disputeMessage, "SentAt", DateTime.Now);

            _context.DisputeMessages.Add(disputeMessage);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = disputeId });
        }

        // POST: /Disputes/Resolve
        [HttpPost]
        public async Task<IActionResult> Resolve(int id, string resolution)
        {
            var dispute = await _context.Disputes
                .Include(d => d.Contract)
                .FirstOrDefaultAsync(d => d.ContractId == id);

            if (dispute == null)
            {
                return NotFound();
            }

            SetEnumPropertyIfExists(dispute, "Status", "Resolved", "Closed", "Close");

            SetPropertyIfExists(dispute, "Resolution", resolution);
            SetPropertyIfExists(dispute, "ResolutionText", resolution);
            SetPropertyIfExists(dispute, "ResolvedAt", DateTime.Now);

            if (dispute.Contract != null)
            {
                dispute.Contract.Status = ContractStatus.Completed;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id });
        }

        private void SetPropertyIfExists(object obj, string propertyName, object value)
        {
            var property = obj.GetType().GetProperty(propertyName);

            if (property != null && property.CanWrite)
            {
                property.SetValue(obj, value);
            }
        }

        private void SetEnumPropertyIfExists(object obj, string propertyName, params string[] possibleValues)
        {
            var property = obj.GetType().GetProperty(propertyName);

            if (property == null || !property.CanWrite || !property.PropertyType.IsEnum)
            {
                return;
            }

            foreach (var value in possibleValues)
            {
                if (Enum.TryParse(property.PropertyType, value, true, out var enumValue))
                {
                    property.SetValue(obj, enumValue);
                    return;
                }
            }
        }
    }
}