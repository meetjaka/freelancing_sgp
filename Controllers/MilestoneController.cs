using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Services.Interfaces;
using System.Security.Claims;

namespace SGP_Freelancing.Controllers
{
    [Authorize]
    public class MilestoneController : Controller
    {
        private readonly IMilestoneService _milestoneService;
        private readonly ILogger<MilestoneController> _logger;

        public MilestoneController(IMilestoneService milestoneService, ILogger<MilestoneController> logger)
        {
            _milestoneService = milestoneService;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateMilestoneDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _milestoneService.CreateMilestoneAsync(dto, userId);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction("Details", "Contract", new { id = dto.ContractId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Fund(int milestoneId, int contractId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _milestoneService.FundMilestoneAsync(milestoneId, userId);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction("Details", "Contract", new { id = contractId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int milestoneId, int contractId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _milestoneService.SubmitMilestoneAsync(milestoneId, userId);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction("Details", "Contract", new { id = contractId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int milestoneId, int contractId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _milestoneService.ApproveMilestoneAsync(milestoneId, userId);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction("Details", "Contract", new { id = contractId });
        }
    }
}
