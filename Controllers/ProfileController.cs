using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Models.Entities;
using SGP_Freelancing.Repositories;
using SGP_Freelancing.Services.Interfaces;
using SGP_Freelancing.Utilities;

namespace SGP_Freelancing.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            IProfileService profileService,
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            ILogger<ProfileController> logger)
        {
            _profileService = profileService;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [Authorize(Policy = Constants.Policies.RequireFreelancerRole)]
        [HttpGet]
        public async Task<IActionResult> EditFreelancer()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            var response = await _profileService.GetFreelancerProfileAsync(userId);
            
            // Get all skills for dropdown
            var skills = await _unitOfWork.Skills.GetAllAsync();
            ViewBag.Skills = skills.OrderBy(s => s.Name).ToList();

            if (response.Success && response.Data != null)
            {
                var model = new UpdateFreelancerProfileDto
                {
                    Title = response.Data.Title,
                    Bio = response.Data.Bio,
                    HourlyRate = response.Data.HourlyRate,
                    PortfolioUrl = response.Data.PortfolioUrl,
                    SkillIds = response.Data.Skills?.Select(s => s.Id).ToList()
                };
                return View(model);
            }
            else
            {
                // New profile - show empty form
                return View(new UpdateFreelancerProfileDto());
            }
        }

        [Authorize(Policy = Constants.Policies.RequireFreelancerRole)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFreelancer(UpdateFreelancerProfileDto model)
        {
            if (!ModelState.IsValid)
            {
                var skills = await _unitOfWork.Skills.GetAllAsync();
                ViewBag.Skills = skills.OrderBy(s => s.Name).ToList();
                return View(model);
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            var response = await _profileService.UpdateFreelancerProfileAsync(userId, model);

            if (response.Success)
            {
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Index", "Dashboard");
            }

            ModelState.AddModelError("", response.Message);
            var skillsList = await _unitOfWork.Skills.GetAllAsync();
            ViewBag.Skills = skillsList.OrderBy(s => s.Name).ToList();
            return View(model);
        }

        [Authorize(Policy = Constants.Policies.RequireClientRole)]
        [HttpGet]
        public async Task<IActionResult> EditClient()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            var response = await _profileService.GetClientProfileAsync(userId);

            if (response.Success && response.Data != null)
            {
                var model = new UpdateClientProfileDto
                {
                    CompanyName = response.Data.CompanyName,
                    CompanyDescription = response.Data.CompanyDescription,
                    Website = response.Data.Website
                };
                return View(model);
            }
            else
            {
                // New profile - show empty form
                return View(new UpdateClientProfileDto());
            }
        }

        [Authorize(Policy = Constants.Policies.RequireClientRole)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditClient(UpdateClientProfileDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            var response = await _profileService.UpdateClientProfileAsync(userId, model);

            if (response.Success)
            {
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Index", "Dashboard");
            }

            ModelState.AddModelError("", response.Message);
            return View(model);
        }
    }
}
