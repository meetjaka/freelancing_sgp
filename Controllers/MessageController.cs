using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Services.Interfaces;
using System.Security.Claims;

namespace SGP_Freelancing.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private readonly IMessageService _messageService;
        private readonly ILogger<MessageController> _logger;

        public MessageController(IMessageService messageService, ILogger<MessageController> logger)
        {
            _messageService = messageService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var viewModel = await _messageService.GetUserMessagesAsync(userId);
                
                ViewBag.UnreadCount = viewModel.UnreadCount;
                
                return View(viewModel.Messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading messages");
                TempData["Error"] = "Error loading messages";
                return View(new List<MessageDto>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Conversation(string userId)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var viewModel = await _messageService.GetConversationAsync(currentUserId, userId);
                
                // Mark messages as read
                var unreadMessages = viewModel.Messages.Where(m => m.ReceiverId == currentUserId && !m.IsRead).Select(m => m.Id);
                foreach (var messageId in unreadMessages)
                {
                    await _messageService.MarkAsReadAsync(messageId, currentUserId);
                }
                
                ViewBag.OtherUserId = userId;
                
                return View(viewModel.Messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading conversation");
                TempData["Error"] = "Error loading conversation";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(SendMessageDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await _messageService.SendMessageAsync(dto, senderId);
                
                if (result.Success)
                {
                    return Json(new { success = true, message = "Message sent" });
                }
                
                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                return Json(new { success = false, message = "Error sending message" });
            }
        }
    }
}
