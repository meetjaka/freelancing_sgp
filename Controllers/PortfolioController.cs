using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Services.Interfaces;
using System.Security.Claims;

namespace SGP_Freelancing.Controllers
{
    [Route("Portfolio")]
    public class PortfolioController : Controller
    {
        private readonly IPortfolioService _portfolioService;
        private readonly ILogger<PortfolioController> _logger;

        public PortfolioController(IPortfolioService portfolioService, ILogger<PortfolioController> logger)
        {
            _portfolioService = portfolioService;
            _logger = logger;
        }

        // GET: Portfolio/Index
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var portfolios = await _portfolioService.GetPublicPortfoliosAsync();
                return View("Index", portfolios.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading portfolios");
                TempData["Error"] = "Error loading portfolios";
                return View("Index", new List<PortfolioDto>());
            }
        }

        // GET: Portfolio/Featured
        [HttpGet("Featured")]
        public async Task<IActionResult> Featured()
        {
            try
            {
                var portfolios = await _portfolioService.GetFeaturedPortfoliosAsync();
                return View("Featured", portfolios.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading featured portfolios");
                TempData["Error"] = "Error loading featured portfolios";
                return View("Featured", new List<PortfolioDto>());
            }
        }

        // GET: Portfolio/Details/{id}
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var portfolio = await _portfolioService.GetPortfolioDetailsAsync(id);
                if (portfolio == null)
                {
                    TempData["Error"] = "Portfolio not found or is not public";
                    return RedirectToAction("Index");
                }
                return View("Details", portfolio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading portfolio details");
                TempData["Error"] = "Error loading portfolio";
                return RedirectToAction("Index");
            }
        }

        // GET: Portfolio/MyPortfolio
        [HttpGet("MyPortfolio")]
        [Authorize]
        public async Task<IActionResult> MyPortfolio()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "User not found";
                    return RedirectToAction("Login", "Account");
                }

                // For now, we'll show a message that portfolio creation is needed
                // In a real app, we'd fetch freelancer profile first
                return View("MyPortfolio");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading my portfolio");
                TempData["Error"] = "Error loading your portfolio";
                return RedirectToAction("Index", "Dashboard");
            }
        }

        // GET: Portfolio/Create
        [HttpGet("Create")]
        [Authorize(Roles = "Freelancer")]
        public IActionResult Create()
        {
            return View("Create", new CreatePortfolioDto());
        }

        // POST: Portfolio/Create
        [HttpPost("Create")]
        [Authorize(Roles = "Freelancer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePortfolioDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View("Create", createDto);

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "User not found";
                    return RedirectToAction("Login", "Account");
                }

                var portfolio = await _portfolioService.CreatePortfolioAsync(userId, createDto);
                
                TempData["Success"] = "Portfolio created successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating portfolio");
                TempData["Error"] = "Error creating portfolio: " + ex.Message;
                return View("Create", createDto);
            }
        }

        // GET: Portfolio/Edit/{id}
        [HttpGet("Edit/{id}")]
        [Authorize(Roles = "Freelancer")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var portfolio = await _portfolioService.GetPortfolioByIdAsync(id);
                if (portfolio == null)
                {
                    TempData["Error"] = "Portfolio not found";
                    return RedirectToAction("MyPortfolio");
                }

                var updateDto = new UpdatePortfolioDto
                {
                    Id = portfolio.Id,
                    Title = portfolio.Title,
                    Description = portfolio.Description,
                    DetailedBio = portfolio.DetailedBio,
                    IsPublic = portfolio.IsPublic,
                    IsFeatured = portfolio.IsFeatured
                };

                return View("Edit", updateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading portfolio edit");
                TempData["Error"] = "Error loading portfolio";
                return RedirectToAction("MyPortfolio");
            }
        }

        // POST: Portfolio/Edit/{id}
        [HttpPost("Edit/{id}")]
        [Authorize(Roles = "Freelancer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdatePortfolioDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View("Edit", updateDto);

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "User not found";
                    return RedirectToAction("Login", "Account");
                }

                await _portfolioService.UpdatePortfolioAsync(id, userId, updateDto);
                TempData["Success"] = "Portfolio updated successfully!";
                return RedirectToAction("Details", new { id });
            }
            catch (UnauthorizedAccessException)
            {
                TempData["Error"] = "You can only edit your own portfolio";
                return RedirectToAction("MyPortfolio");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating portfolio");
                TempData["Error"] = "Error updating portfolio: " + ex.Message;
                return View("Edit", updateDto);
            }
        }

        // POST: Portfolio/Delete/{id}
        [HttpPost("Delete/{id}")]
        [Authorize(Roles = "Freelancer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "User not found";
                    return RedirectToAction("Login", "Account");
                }

                await _portfolioService.DeletePortfolioAsync(id, userId);
                TempData["Success"] = "Portfolio deleted successfully!";
                return RedirectToAction("MyPortfolio");
            }
            catch (UnauthorizedAccessException)
            {
                TempData["Error"] = "You can only delete your own portfolio";
                return RedirectToAction("MyPortfolio");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting portfolio");
                TempData["Error"] = "Error deleting portfolio: " + ex.Message;
                return RedirectToAction("Details", new { id });
            }
        }

        // GET: Portfolio/AddCase/{portfolioId}
        [HttpGet("AddCase/{portfolioId}")]
        [Authorize(Roles = "Freelancer")]
        public IActionResult AddCase(int portfolioId)
        {
            var createDto = new CreatePortfolioCaseDto { PortfolioId = portfolioId };
            return View("AddCase", createDto);
        }

        // POST: Portfolio/AddCase
        [HttpPost("AddCase")]
        [Authorize(Roles = "Freelancer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCase(CreatePortfolioCaseDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View("AddCase", createDto);

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "User not found";
                    return RedirectToAction("Login", "Account");
                }

                var portfolioCase = await _portfolioService.CreatePortfolioCaseAsync(userId, createDto);
                TempData["Success"] = "Case study added successfully!";
                return RedirectToAction("Edit", new { id = createDto.PortfolioId });
            }
            catch (UnauthorizedAccessException)
            {
                TempData["Error"] = "You can only add cases to your own portfolio";
                return View("AddCase", createDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding case study");
                TempData["Error"] = "Error adding case study: " + ex.Message;
                return View("AddCase", createDto);
            }
        }

        // GET: Portfolio/EditCase/{caseId}
        [HttpGet("EditCase/{caseId}")]
        [Authorize(Roles = "Freelancer")]
        public async Task<IActionResult> EditCase(int caseId)
        {
            try
            {
                var portfolioCase = await _portfolioService.GetPortfolioCaseAsync(caseId);
                if (portfolioCase == null)
                {
                    TempData["Error"] = "Case study not found";
                    return RedirectToAction("MyPortfolio");
                }

                var updateDto = new UpdatePortfolioCaseDto
                {
                    Id = portfolioCase.Id,
                    Title = portfolioCase.Title,
                    Description = portfolioCase.Description,
                    DetailedDescription = portfolioCase.Description,
                    ClientName = portfolioCase.ClientName,
                    Industry = portfolioCase.Industry,
                    ProjectUrl = portfolioCase.ProjectUrl,
                    BudgetAmount = portfolioCase.BudgetAmount,
                    BudgetCurrency = portfolioCase.BudgetCurrency,
                    CompletionDate = portfolioCase.CompletionDate,
                    Technologies = portfolioCase.Technologies,
                    Rating = portfolioCase.Rating,
                    IsHighlighted = portfolioCase.IsHighlighted,
                    DisplayOrder = portfolioCase.DisplayOrder
                };

                return View("EditCase", updateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading case edit");
                TempData["Error"] = "Error loading case";
                return RedirectToAction("MyPortfolio");
            }
        }

        // POST: Portfolio/EditCase
        [HttpPost("EditCase")]
        [Authorize(Roles = "Freelancer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCase(UpdatePortfolioCaseDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View("EditCase", updateDto);

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "User not found";
                    return RedirectToAction("Login", "Account");
                }

                await _portfolioService.UpdatePortfolioCaseAsync(updateDto.Id, userId, updateDto);
                TempData["Success"] = "Case study updated successfully!";
                return RedirectToAction("MyPortfolio");
            }
            catch (UnauthorizedAccessException)
            {
                TempData["Error"] = "You can only edit your own cases";
                return View("EditCase", updateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating case study");
                TempData["Error"] = "Error updating case study: " + ex.Message;
                return View("EditCase", updateDto);
            }
        }

        // POST: Portfolio/DeleteCase/{caseId}
        [HttpPost("DeleteCase/{caseId}")]
        [Authorize(Roles = "Freelancer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCase(int caseId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "User not found";
                    return RedirectToAction("Login", "Account");
                }

                await _portfolioService.DeletePortfolioCaseAsync(caseId, userId);
                TempData["Success"] = "Case study deleted successfully!";
                return RedirectToAction("MyPortfolio");
            }
            catch (UnauthorizedAccessException)
            {
                TempData["Error"] = "You can only delete your own cases";
                return RedirectToAction("MyPortfolio");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting case study");
                TempData["Error"] = "Error deleting case study: " + ex.Message;
                return RedirectToAction("MyPortfolio");
            }
        }

        // POST: Portfolio/AddImage
        [HttpPost("AddImage")]
        [Authorize(Roles = "Freelancer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddImage(CreatePortfolioImageDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Invalid image data" });

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Json(new { success = false, message = "User not found" });

                var portfolioImage = await _portfolioService.AddPortfolioImageAsync(userId, createDto);
                return Json(new { success = true, message = "Image added successfully", data = portfolioImage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding portfolio image");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Portfolio/DeleteImage/{imageId}
        [HttpPost("DeleteImage/{imageId}")]
        [Authorize(Roles = "Freelancer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Json(new { success = false, message = "User not found" });

                await _portfolioService.DeletePortfolioImageAsync(imageId, userId);
                return Json(new { success = true, message = "Image deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting portfolio image");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Portfolio/AddTestimonial
        [HttpPost("AddTestimonial")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTestimonial(CreateProjectTestimonialDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Invalid testimonial data" });

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
                var testimonial = await _portfolioService.AddTestimonialAsync(userId, createDto);
                return Json(new { success = true, message = "Testimonial added successfully", data = testimonial });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding testimonial");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Portfolio/DeleteTestimonial/{testimonialId}
        [HttpPost("DeleteTestimonial/{testimonialId}")]
        [Authorize(Roles = "Freelancer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTestimonial(int testimonialId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Json(new { success = false, message = "User not found" });

                await _portfolioService.DeleteTestimonialAsync(testimonialId, userId);
                return Json(new { success = true, message = "Testimonial deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting testimonial");
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}

