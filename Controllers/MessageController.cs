using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Models.Entities;
using SGP_Freelancing.Repositories;
using SGP_Freelancing.Services.Interfaces;
using System.Security.Claims;

namespace SGP_Freelancing.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private readonly IMessageService _messageService;
        private readonly IFileUploadService _fileUploadService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MessageController> _logger;

        public MessageController(
            IMessageService messageService,
            IFileUploadService fileUploadService,
            IUnitOfWork unitOfWork,
            ILogger<MessageController> logger)
        {
            _messageService = messageService;
            _fileUploadService = fileUploadService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult New(string recipientId)
        {
            if (string.IsNullOrWhiteSpace(recipientId))
            {
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Conversation), new { userId = recipientId });
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var viewModel = await _messageService.GetUserMessagesAsync(currentUserId);
                
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
        public async Task<IActionResult> Conversation(string userId, int page = 1, int pageSize = 20)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var canMessage = await _messageService.CanMessageAsync(currentUserId, userId);

                if (!canMessage.Success && !canMessage.Message.Contains("closed", StringComparison.OrdinalIgnoreCase))
                {
                    TempData["Error"] = canMessage.Message;
                    return RedirectToAction(nameof(Index));
                }

                // Get paginated messages
                var pagedResult = await _messageService.GetConversationPaginatedAsync(currentUserId, userId, page, pageSize);

                // Mark current page's unread messages as read
                var unreadMessages = pagedResult.Items.Where(m => m.ReceiverId == currentUserId && !m.IsRead).Select(m => m.Id);
                foreach (var messageId in unreadMessages)
                {
                    await _messageService.MarkAsReadAsync(messageId, currentUserId);
                }

                ViewBag.OtherUserId = userId;
                ViewBag.CanSend = canMessage.Success;
                ViewBag.ChatStatusMessage = canMessage.Success ? null : canMessage.Message;

                return View(pagedResult);
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
                var canMessage = await _messageService.CanMessageAsync(senderId, dto.ReceiverId);
                if (!canMessage.Success)
                {
                    return Json(new { success = false, message = canMessage.Message });
                }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadFile(string receiverId, IFormFile? file, string? caption)
        {
            if (string.IsNullOrWhiteSpace(receiverId))
            {
                return Json(new { success = false, message = "Receiver is required." });
            }

            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "Please choose a file to upload." });
            }

            try
            {
                var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var canMessage = await _messageService.CanMessageAsync(senderId, receiverId);
                if (!canMessage.Success)
                {
                    return Json(new { success = false, message = canMessage.Message });
                }

                var messageContent = string.IsNullOrWhiteSpace(caption)
                    ? $"Shared a file: {file.FileName}"
                    : caption.Trim();

                var sendResult = await _messageService.SendMessageAsync(new SendMessageDto
                {
                    ReceiverId = receiverId,
                    Subject = "Project File",
                    Content = messageContent
                }, senderId);

                if (!sendResult.Success || sendResult.Data == null)
                {
                    return Json(new { success = false, message = sendResult.Message });
                }

                var uploadResult = await _fileUploadService.UploadFileAsync(file, senderId, messageId: sendResult.Data.Id);
                if (!uploadResult.Success || uploadResult.Data == null)
                {
                    return Json(new { success = false, message = uploadResult.Message });
                }

                return Json(new
                {
                    success = true,
                    message = "File sent successfully.",
                    sentMessage = sendResult.Data,
                    attachment = uploadResult.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file in conversation");
                return Json(new { success = false, message = "Error uploading file." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadAttachment(int fileId)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                var attachment = await _unitOfWork.Repository<FileAttachment>()
                    .Query()
                    .Include(f => f.Message)
                    .FirstOrDefaultAsync(f => f.Id == fileId && !f.IsDeleted);

                if (attachment == null || !attachment.MessageId.HasValue || attachment.Message == null)
                {
                    return NotFound();
                }

                var canAccess = attachment.Message.SenderId == currentUserId || attachment.Message.ReceiverId == currentUserId;
                if (!canAccess)
                {
                    return Forbid();
                }

                var relativePath = attachment.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);

                if (!System.IO.File.Exists(physicalPath))
                {
                    return NotFound();
                }

                return PhysicalFile(physicalPath, attachment.FileType ?? "application/octet-stream", attachment.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading attachment {AttachmentId}", fileId);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseConversation(string otherUserId)
        {
            if (string.IsNullOrWhiteSpace(otherUserId))
            {
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await _messageService.CloseConversationAsync(currentUserId, otherUserId);

                if (result.Success)
                {
                    TempData["Success"] = result.Message;
                }
                else
                {
                    TempData["Error"] = result.Message;
                }

                return RedirectToAction(nameof(Conversation), new { userId = otherUserId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing conversation");
                TempData["Error"] = "Failed to close conversation.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMessage(int messageId, string otherUserId)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await _messageService.DeleteMessageAsync(messageId, currentUserId);

                if (result.Success)
                {
                    TempData["Success"] = result.Message;
                }
                else
                {
                    TempData["Error"] = result.Message;
                }

                if (!string.IsNullOrWhiteSpace(otherUserId))
                {
                    return RedirectToAction(nameof(Conversation), new { userId = otherUserId });
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message {MessageId}", messageId);
                TempData["Error"] = "Failed to delete message.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConversation(string otherUserId)
        {
            if (string.IsNullOrWhiteSpace(otherUserId))
            {
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await _messageService.DeleteConversationAsync(currentUserId, otherUserId);

                if (result.Success)
                {
                    TempData["Success"] = result.Message;
                }
                else
                {
                    TempData["Error"] = result.Message;
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting conversation with {OtherUserId}", otherUserId);
                TempData["Error"] = "Failed to delete conversation.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
