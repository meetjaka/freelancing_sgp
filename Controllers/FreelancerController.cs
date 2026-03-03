using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Repositories;
using SGP_Freelancing.Services.Interfaces;

namespace SGP_Freelancing.Controllers
{
    public class FreelancerController : Controller
    {
        private readonly IFreelancerService _freelancerService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FreelancerController> _logger;

        public FreelancerController(
            IFreelancerService freelancerService,
            IUnitOfWork unitOfWork,
            ILogger<FreelancerController> logger)
        {
            _freelancerService = freelancerService;
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
