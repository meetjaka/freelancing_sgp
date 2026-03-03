using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Models.Entities;
using System.Security.Claims;

namespace SGP_Freelancing.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly ILogger<SettingsController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public SettingsController(ILogger<SettingsController> logger, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.Identity?.Name ?? "User";
            
            ViewBag.UserName = userName;
            ViewBag.UserId = userId;
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill all required fields correctly.";
                return RedirectToAction("Index");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Index");
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Password changed successfully!";
                _logger.LogInformation("User {UserId} changed their password successfully.", user.Id);
            }
            else
            {
                string errors = string.Join(", ", result.Errors.Select(e => e.Description));
                TempData["ErrorMessage"] = errors;
                _logger.LogWarning("Failed to change password for user {UserId}: {Errors}", user.Id, errors);
            }

            return RedirectToAction("Index");
        }
    }
}
