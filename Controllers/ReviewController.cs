using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Services.Interfaces;
using System.Security.Claims;

namespace SGP_Freelancing.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly IReviewService _reviewService;
        private readonly ILogger<ReviewController> _logger;

        public ReviewController(IReviewService reviewService, ILogger<ReviewController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReview(CreateReviewDto dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid review data.";
                return RedirectToAction("Details", "Contract", new { id = dto.ContractId });
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _reviewService.CreateReviewAsync(dto, userId);

                if (result.Success)
                {
                    TempData["Success"] = result.Message;
                }
                else
                {
                    TempData["Error"] = result.Message;
                }

                return RedirectToAction("Details", "Contract", new { id = dto.ContractId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting review");
                TempData["Error"] = "An error occurred while submitting your review.";
                return RedirectToAction("Details", "Contract", new { id = dto.ContractId });
            }
        }
    }
}
