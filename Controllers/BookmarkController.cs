using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Models.Entities;
using SGP_Freelancing.Services.Interfaces;
using System.Security.Claims;

namespace SGP_Freelancing.Controllers
{
    [Authorize]
    public class BookmarkController : Controller
    {
        private readonly IBookmarkService _bookmarkService;
        private readonly ILogger<BookmarkController> _logger;

        public BookmarkController(IBookmarkService bookmarkService, ILogger<BookmarkController> logger)
        {
            _bookmarkService = bookmarkService;
            _logger = logger;
        }

        /// <summary>
        /// View all saved/bookmarked items
        /// </summary>
        public async Task<IActionResult> Index(string? type = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var userName = User.Identity?.Name ?? "User";
                
                ViewBag.UserName = userName;
                ViewBag.UserId = userId;
                ViewBag.ActiveTab = type ?? "all";

                var response = await _bookmarkService.GetUserBookmarksAsync(userId, type);
                
                if (response.Success && response.Data != null)
                {
                    return View(response.Data);
                }

                return View(new List<BookmarkDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading bookmarks");
                return View(new List<BookmarkDto>());
            }
        }

        /// <summary>
        /// Toggle bookmark (add/remove) - AJAX endpoint
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle([FromBody] CreateBookmarkDto dto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await _bookmarkService.ToggleBookmarkAsync(dto, userId);
                
                return Json(new { 
                    success = result.Success, 
                    message = result.Message,
                    isBookmarked = result.Message == "Bookmark added"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling bookmark");
                return Json(new { success = false, message = "An error occurred" });
            }
        }

        /// <summary>
        /// Check if an item is bookmarked - AJAX endpoint
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> IsBookmarked(string bookmarkType, int itemId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await _bookmarkService.IsBookmarkedAsync(userId, bookmarkType, itemId);
                
                return Json(new { isBookmarked = result.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking bookmark");
                return Json(new { isBookmarked = false });
            }
        }

        /// <summary>
        /// Get all bookmarked item IDs for a type - AJAX endpoint
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetBookmarkedIds(string bookmarkType)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await _bookmarkService.GetBookmarkedItemIdsAsync(userId, bookmarkType);
                
                return Json(new { ids = result.Data ?? new List<int>() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookmark IDs");
                return Json(new { ids = new List<int>() });
            }
        }

        /// <summary>
        /// Remove a bookmark - AJAX endpoint
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await _bookmarkService.RemoveBookmarkAsync(id, userId);
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = result.Success, message = result.Message });
                }

                if (result.Success)
                    TempData["Success"] = result.Message;
                else
                    TempData["Error"] = result.Message;

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing bookmark");
                TempData["Error"] = "An error occurred";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
