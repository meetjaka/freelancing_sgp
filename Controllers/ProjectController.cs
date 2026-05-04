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
        private readonly IFileUploadService _fileUploadService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMLService _mlService;
        private readonly ILogger<ProjectController> _logger;

        public ProjectController(
            IProjectService projectService, 
            IBidService bidService, 
            IBookmarkService bookmarkService,
            IFileUploadService fileUploadService,
            IUnitOfWork unitOfWork,
            IMLService mlService,
            ILogger<ProjectController> logger)
        {
            _projectService = projectService;
            _bidService = bidService;
            _bookmarkService = bookmarkService;
            _fileUploadService = fileUploadService;
            _unitOfWork = unitOfWork;
            _mlService = mlService;
            _logger = logger;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(ProjectSearchDto searchDto)
        {
            try
            {
                PagedResult<ProjectDto> result;

                // If user is searching by text, use our shiny new AI vector search!
                if (!string.IsNullOrWhiteSpace(searchDto.SearchTerm))
                {
                    var aiRequest = new SemanticSearchRequest 
                    { 
                        Query = searchDto.SearchTerm, 
                        TopN = 30 
                    };
                    var aiResponse = await _mlService.SearchAsync(aiRequest);
                    
                    var aiProjects = aiResponse?.Results.Select(r => new ProjectDto
                    {
                        Id = -1 * new Random().Next(1000, 9999), // Negative ID signals "Market Deal"
                        Title = r.Title,
                        Description = r.DescriptionSnippet + " [Market Data extracted from AI Database]",
                        CategoryName = r.Category,
                        Budget = r.Budget,
                        Status = "Open",
                        CreatedAt = DateTime.UtcNow,
                        ClientName = "Market Deal (AI Match)",
                        ClientProjectsCount = 1,
                        Skills = new List<string>()
                        // We use a fake ID so it renders seamlessly in our existing views.
                    }).ToList() ?? new List<ProjectDto>();

                    result = new PagedResult<ProjectDto> 
                    {
                        Items = aiProjects,
                        TotalCount = aiProjects.Count,
                        PageNumber = 1,
                        PageSize = 30
                    };
                    
                    if (aiProjects.Any())
                        TempData["Success"] = $"AI Semantic Search found {aiProjects.Count} relevant matches based on your phrasing!";
                }
                else
                {
                    // Fall back to standard SQL Server logic
                    result = await _projectService.AdvancedSearchAsync(searchDto);
                }
                
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
        public async Task<IActionResult> Create()
        {
            var skills = await _unitOfWork.Skills.GetAllAsync();
            ViewBag.Skills = skills.OrderBy(s => s.Name).ToList();
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProjectDto dto, List<IFormFile>? projectFiles)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                // --- AI Spam Detection ---
                var spamRequest = new SpamCheckRequest
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    Budget = dto.Budget
                };
                
                var spamResult = await _mlService.CheckSpamAsync(spamRequest);
                if (spamResult != null && spamResult.IsSpam && spamResult.RiskScore > 75)
                {
                    _logger.LogWarning("Spam project blocked: Risk {RiskScore}. Title: {Title}", spamResult.RiskScore, dto.Title);
                    ModelState.AddModelError("", $"Project blocked by AI Anti-Spam filter (Risk {spamResult.RiskScore}%). Reason: {spamResult.Message}");
                    return View(dto);
                }
                // -------------------------

                var result = await _projectService.CreateProjectAsync(dto, userId);
                
                if (result.Success)
                {
                    if (projectFiles != null && projectFiles.Any() && result.Data != null)
                    {
                        foreach (var file in projectFiles.Where(f => f != null && f.Length > 0))
                        {
                            var uploadResult = await _fileUploadService.UploadFileAsync(file, userId, projectId: result.Data.Id);
                            if (!uploadResult.Success)
                            {
                                _logger.LogWarning("Project file upload failed for project {ProjectId}: {Message}", result.Data.Id, uploadResult.Message);
                            }
                        }
                    }

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
