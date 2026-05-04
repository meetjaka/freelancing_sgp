using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Models.ViewModels;
using SGP_Freelancing.Services.Interfaces;
using System.Security.Claims;

namespace SGP_Freelancing.Controllers
{
    [Authorize]
    public class VerificationController : Controller
    {
        private readonly IVerificationService _verificationService;
        private readonly ILogger<VerificationController> _logger;

        public VerificationController(IVerificationService verificationService, ILogger<VerificationController> logger)
        {
            _verificationService = verificationService;
            _logger = logger;
        }

        public async Task<IActionResult> Status()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var status = await _verificationService.GetStatusAsync(userId);
            var vm = new VerificationViewModel
            {
                Status = status,
                HasUploaded = !string.IsNullOrEmpty(status.DocumentUrl)
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(string documentUrl)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _verificationService.SubmitDocumentAsync(userId, documentUrl);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Status));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminIndex()
        {
            var verifications = await _verificationService.GetPendingVerificationsAsync();
            return View(verifications);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(string userId, string? note)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _verificationService.ApproveVerificationAsync(userId, adminId, note);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(AdminIndex));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(string userId, string note)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _verificationService.RejectVerificationAsync(userId, adminId, note);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(AdminIndex));
        }
    }
}
