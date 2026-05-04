using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Models.ViewModels;
using SGP_Freelancing.Services.Interfaces;
using System.Security.Claims;

namespace SGP_Freelancing.Controllers
{
    [Authorize]
    public class DisputeController : Controller
    {
        private readonly IDisputeService _disputeService;
        private readonly ILogger<DisputeController> _logger;

        public DisputeController(IDisputeService disputeService, ILogger<DisputeController> logger)
        {
            _disputeService = disputeService;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDisputeDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _disputeService.RaiseDisputeAsync(dto, userId);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction("Details", "Contract", new { id = dto.ContractId });
        }

        public async Task<IActionResult> Details(int id)
        {
            var dispute = await _disputeService.GetDisputeDetailsAsync(id);
            if (dispute == null) return NotFound();

            var vm = new DisputeDetailsViewModel
            {
                Dispute = dispute,
                IsAdmin = User.IsInRole("Admin"),
                CanResolve = User.IsInRole("Admin") && dispute.Status != "Resolved" && dispute.Status != "Closed"
            };
            return View(vm);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var disputes = await _disputeService.GetAllPendingDisputesAsync();
            return View(disputes);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Resolve(ResolveDisputeDto dto)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _disputeService.ResolveDisputeAsync(dto, adminId);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
