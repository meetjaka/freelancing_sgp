using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Models.ViewModels;
using SGP_Freelancing.Services.Interfaces;
using System.Security.Claims;

namespace SGP_Freelancing.Controllers
{
    [Authorize]
    public class TimeTrackingController : Controller
    {
        private readonly ITimeTrackingService _timeTrackingService;
        private readonly IContractService _contractService;
        private readonly ILogger<TimeTrackingController> _logger;

        public TimeTrackingController(ITimeTrackingService timeTrackingService, IContractService contractService, ILogger<TimeTrackingController> logger)
        {
            _timeTrackingService = timeTrackingService;
            _contractService = contractService;
            _logger = logger;
        }

        public async Task<IActionResult> Timesheet(int contractId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var summary = await _timeTrackingService.GetTimesheetAsync(contractId);
            var contractDetails = await _contractService.GetContractDetailsAsync(contractId, userId);

            var vm = new TimesheetViewModel
            {
                Summary = summary,
                Contract = contractDetails?.Contract ?? new ContractDto(),
                IsFreelancer = User.IsInRole("Freelancer")
            };
            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Freelancer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogHours(CreateTimeEntryDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _timeTrackingService.LogHoursAsync(dto, userId);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Timesheet), new { contractId = dto.ContractId });
        }

        [HttpPost]
        [Authorize(Roles = "Client")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int entryId, int contractId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _timeTrackingService.ApproveEntryAsync(entryId, userId);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Timesheet), new { contractId });
        }

        [HttpPost]
        [Authorize(Roles = "Client")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int entryId, int contractId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _timeTrackingService.RejectEntryAsync(entryId, userId);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Timesheet), new { contractId });
        }
    }
}
