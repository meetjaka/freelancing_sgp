using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Models.ViewModels;
using SGP_Freelancing.Repositories;
using SGP_Freelancing.Services.Interfaces;
using SGP_Freelancing.Utilities;
using System.Security.Claims;

namespace SGP_Freelancing.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {
        private readonly IProjectService _projectService;
        private readonly IBidService _bidService;
        private readonly IBookmarkService _bookmarkService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProjectController> _logger;

        public ProjectController(
            IProjectService projectService, 
            IBidService bidService, 
            IBookmarkService bookmarkService,
            IUnitOfWork unitOfWork,
            ILogger<ProjectController> logger)
        {
            _projectService = projectService;
            _bidService = bidService;
            _bookmarkService = bookmarkService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(ProjectSearchDto searchDto)
        {
            try
            {
                var result = await _projectService.AdvancedSearchAsync(searchDto);
                
                ViewBag.SearchDto = searchDto;
                
                // Get categories and skills for filter dropdowns
                var categories = await _unitOfWork.Categories.GetAllAsync();
                var skills = await _unitOfWork.Skills.GetAllAsync();
                ViewBag.Categories = categories.OrderBy(c => c.Name).ToList();
                ViewBag.Skills = skills.OrderBy(s => s.Name).ToList();
                
                // Get bookmarked project IDs if user is authenticated
                if (User.Identity?.IsAuthenticated == true)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                    ViewBag.BookmarkedProjectIds = await _bookmarkService.GetBookmarkedItemIdsAsync(userId, "Project");
                }
                
                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading projects");
                TempData["Error"] = "Error loading projects";
                return View(new PagedResult<ProjectDto>());
            }
        }

        [AllowAnonymous]
        [Route("Project/Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var viewModel = await _projectService.GetProjectDetailsAsync(id, userId);
                
                if (viewModel == null)
                    return NotFound();
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading project details");
                return NotFound();
            }
        }

        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProjectDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await _projectService.CreateProjectAsync(dto, userId);
                
                if (result.Success)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToAction(nameof(Details), new { id = result.Data!.Id });
                }
                
                ModelState.AddModelError("", result.Message);
                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project");
                ModelState.AddModelError("", "An error occurred while creating the project");
                return View(dto);
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var project = await _projectService.GetProjectByIdAsync(id);
                if (project == null)
                    return NotFound();

                var dto = new UpdateProjectDto
                {
                    Id = project.Id,
                    Title = project.Title,
                    Description = project.Description,
                    Budget = project.Budget,
                    Deadline = project.Deadline,
                    CategoryId = 1 // You'll need to get this from project
                };

                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading project for edit");
                return NotFound();
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateProjectDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await _projectService.UpdateProjectAsync(dto, userId);
                
                if (result.Success)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToAction(nameof(Details), new { id = dto.Id });
                }
                
                ModelState.AddModelError("", result.Message);
                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating project");
                ModelState.AddModelError("", "An error occurred while updating the project");
                return View(dto);
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await _projectService.DeleteProjectAsync(id, userId);
                
                if (result.Success)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }
                
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting project");
                TempData["Error"] = "An error occurred while deleting the project";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitBid(CreateBidDto dto)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Details), new { id = dto.ProjectId });

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await _bidService.CreateBidAsync(dto, userId);
                
                if (result.Success)
                {
                    TempData["Success"] = result.Message;
                }
                else
                {
                    TempData["Error"] = result.Message;
                }
                
                return RedirectToAction(nameof(Details), new { id = dto.ProjectId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting bid");
                TempData["Error"] = "An error occurred while submitting your bid";
                return RedirectToAction(nameof(Details), new { id = dto.ProjectId });
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptBid(int bidId, int projectId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await _bidService.AcceptBidAsync(bidId, userId);
                
                if (result.Success)
                {
                    TempData["Success"] = result.Message;
                }
                else
                {
                    TempData["Error"] = result.Message;
                }
                
                return RedirectToAction(nameof(Details), new { id = projectId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting bid");
                TempData["Error"] = "An error occurred while accepting the bid";
                return RedirectToAction(nameof(Details), new { id = projectId });
            }
        }

        [Authorize]
        public async Task<IActionResult> MyProjects()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var projects = await _projectService.GetProjectsByClientAsync(userId);
                
                return View(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading client projects");
                TempData["Error"] = "Error loading your projects";
                return View(new List<ProjectDto>());
            }
        }

        [Authorize]
        public async Task<IActionResult> Recommended()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var recommendedProjects = await _projectService.GetRecommendedProjectsAsync(userId, 10);
                
                return View(recommendedProjects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading recommended projects");
                TempData["Error"] = "Error loading your recommended projects";
                return View(new List<ProjectDto>());
            }
        }
    }
}
