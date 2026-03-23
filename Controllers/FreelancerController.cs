using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Repositories;
using SGP_Freelancing.Services.Interfaces;
using System.Security.Claims;

namespace SGP_Freelancing.Controllers
{
    public class FreelancerController : Controller
    {
        private readonly IFreelancerService _freelancerService;
        private readonly IBookmarkService _bookmarkService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FreelancerController> _logger;

        public FreelancerController(
            IFreelancerService freelancerService,
            IBookmarkService bookmarkService,
            IUnitOfWork unitOfWork,
            ILogger<FreelancerController> logger)
        {
            _freelancerService = freelancerService;
            _bookmarkService = bookmarkService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(FreelancerSearchDto searchDto)
        {
            // Get all skills for filter dropdown
            var skills = await _unitOfWork.Skills.GetAllAsync();
            ViewBag.Skills = skills.OrderBy(s => s.Name).ToList();

            var response = await _freelancerService.SearchFreelancersAsync(searchDto);

            // Get bookmarked freelancer IDs if user is authenticated
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                ViewBag.BookmarkedFreelancerIds = await _bookmarkService.GetBookmarkedItemIdsAsync(userId, "Freelancer");
            }

            if (response.Success && response.Data != null)
            {
                ViewBag.SearchDto = searchDto;
                return View(response.Data);
            }

            ViewBag.ErrorMessage = response.Message;
            return View(new PagedResult<FreelancerProfileDto>());
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var response = await _freelancerService.GetFreelancerDetailAsync(id);

            if (response.Success && response.Data != null)
            {
                return View(response.Data);
            }

            TempData["ErrorMessage"] = response.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
