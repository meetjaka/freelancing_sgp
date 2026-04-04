using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Models.Entities;
using SGP_Freelancing.Models.ViewModels;
using SGP_Freelancing.Repositories;
using SGP_Freelancing.Services.Interfaces;
using SGP_Freelancing.Utilities;

namespace SGP_Freelancing.Services
{
    public class MessageService : IMessageService
    {
        private const string ClosedConversationSubject = "SYSTEM_CONVERSATION_CLOSED";
        private const string ClosedConversationContent = "Conversation closed by client after project completion.";

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<MessageService> _logger;

        public MessageService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<MessageService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<MessageDto>> SendMessageAsync(SendMessageDto dto, string senderId)
        {
            try
            {
                var canMessage = await CanMessageAsync(senderId, dto.ReceiverId);
                if (!canMessage.Success)
                {
                    return ApiResponse<MessageDto>.ErrorResponse(canMessage.Message);
                }

                var message = _mapper.Map<Message>(dto);
                message.SenderId = senderId;
                message.Subject = string.IsNullOrWhiteSpace(dto.Subject) ? "Project Discussion" : dto.Subject.Trim();
                message.IsRead = false;

                await _unitOfWork.Messages.AddAsync(message);
                await _unitOfWork.SaveChangesAsync();

                var messageDto = _mapper.Map<MessageDto>(message);
                return ApiResponse<MessageDto>.SuccessResponse(messageDto, Constants.Messages.MessageSent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                return ApiResponse<MessageDto>.ErrorResponse("Failed to send message");
            }
        }

        public async Task<MessagesViewModel> GetUserMessagesAsync(string userId)
        {
            var messages = await _unitOfWork.Messages.GetUserMessagesAsync(userId);
            var filteredMessages = messages.Where(m => m.Subject != ClosedConversationSubject).ToList();
            var unreadCount = await _unitOfWork.Messages.GetUnreadCountAsync(userId);

            var conversationMessages = filteredMessages
                .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Select(g =>
                {
                    var latestMessage = g.OrderByDescending(x => x.CreatedAt).First();
                    var hasUnread = g.Any(x => x.ReceiverId == userId && !x.IsRead);
                    latestMessage.IsRead = !hasUnread;
                    return latestMessage;
                })
                .OrderByDescending(m => m.CreatedAt)
                .ToList();

            return new MessagesViewModel
            {
                Messages = _mapper.Map<List<MessageDto>>(conversationMessages),
                UnreadCount = unreadCount
            };
        }

        public async Task<ConversationViewModel> GetConversationAsync(string userId, string otherUserId)
        {
            var messages = await _unitOfWork.Messages.GetConversationAsync(userId, otherUserId);
            var filteredMessages = messages.Where(m => m.Subject != ClosedConversationSubject).ToList();
            var otherUser = await _unitOfWork.Repository<ApplicationUser>().FirstOrDefaultAsync(u => u.Id == otherUserId);

            return new ConversationViewModel
            {
                Messages = _mapper.Map<List<MessageDto>>(filteredMessages),
                OtherUserId = otherUserId,
                OtherUserName = otherUser != null ? $"{otherUser.FirstName} {otherUser.LastName}" : "Unknown"
            };
        }

        public async Task<PagedResult<MessageDto>> GetConversationPaginatedAsync(string userId, string otherUserId, int page = 1, int pageSize = 20)
        {
            try
            {
                var allMessages = await _unitOfWork.Messages.GetConversationAsync(userId, otherUserId);
                var orderedMessages = allMessages
                    .Where(m => m.Subject != ClosedConversationSubject)
                    .OrderByDescending(m => m.CreatedAt)
                    .ToList();

                var totalCount = orderedMessages.Count;
                var pagedMessages = orderedMessages
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .OrderBy(m => m.CreatedAt) // Display in chronological order within page
                    .ToList();

                var messageDtos = _mapper.Map<List<MessageDto>>(pagedMessages);

                var messageIds = pagedMessages.Select(m => m.Id).ToList();
                if (messageIds.Any())
                {
                    var attachments = await _unitOfWork.Repository<FileAttachment>()
                        .Query()
                        .Include(f => f.UploadedBy)
                        .Where(f => f.MessageId.HasValue && messageIds.Contains(f.MessageId.Value) && !f.IsDeleted)
                        .OrderBy(f => f.CreatedAt)
                        .ToListAsync();

                    var attachmentDtos = _mapper.Map<List<FileAttachmentDto>>(attachments);
                    var attachmentLookup = attachmentDtos
                        .Where(a => a.MessageId.HasValue)
                        .GroupBy(a => a.MessageId!.Value)
                        .ToDictionary(g => g.Key, g => g.ToList());

                    foreach (var messageDto in messageDtos)
                    {
                        if (attachmentLookup.TryGetValue(messageDto.Id, out var groupedAttachments))
                        {
                            messageDto.Attachments = groupedAttachments;
                        }
                    }
                }

                return new PagedResult<MessageDto>
                {
                    Items = messageDtos,
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting paginated conversation between {userId} and {otherUserId}");
                return new PagedResult<MessageDto>
                {
                    Items = new List<MessageDto>(),
                    TotalCount = 0,
                    PageNumber = page,
                    PageSize = pageSize
                };
            }
        }

        public async Task<ApiResponse<bool>> MarkAsReadAsync(int messageId, string userId)
        {
            try
            {
                var message = await _unitOfWork.Messages.GetByIdAsync(messageId);
                if (message == null)
                    return ApiResponse<bool>.ErrorResponse("Message not found");

                if (message.ReceiverId != userId)
                    return ApiResponse<bool>.ErrorResponse(Constants.ErrorMessages.Unauthorized);

                message.IsRead = true;
                message.ReadAt = DateTime.UtcNow;

                _unitOfWork.Messages.Update(message);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Message marked as read");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking message as read");
                return ApiResponse<bool>.ErrorResponse("Failed to mark message as read");
            }
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _unitOfWork.Messages.GetUnreadCountAsync(userId);
        }

        public async Task<ApiResponse<bool>> CanMessageAsync(string userId, string otherUserId)
        {
            try
            {
                var allContracts = await _unitOfWork.Contracts.GetAllAsync();
                var hasContract = allContracts.Any(c =>
                    ((c.ClientId == userId && c.FreelancerId == otherUserId) ||
                     (c.ClientId == otherUserId && c.FreelancerId == userId)) &&
                    (c.Status == ContractStatus.Active || c.Status == ContractStatus.Completed));

                if (!hasContract)
                {
                    return ApiResponse<bool>.ErrorResponse("Messaging is enabled only after a bid is accepted and a contract is created.");
                }

                var closureMessages = await _unitOfWork.Messages.FindAsync(m =>
                    ((m.SenderId == userId && m.ReceiverId == otherUserId) ||
                     (m.SenderId == otherUserId && m.ReceiverId == userId)) &&
                    m.Subject == ClosedConversationSubject);

                if (closureMessages.Any())
                {
                    return ApiResponse<bool>.ErrorResponse("This conversation has been closed by the client after project completion.");
                }

                return ApiResponse<bool>.SuccessResponse(true, "Messaging is allowed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking message permission between {UserId} and {OtherUserId}", userId, otherUserId);
                return ApiResponse<bool>.ErrorResponse("Unable to verify messaging permissions right now.");
            }
        }

        public async Task<ApiResponse<bool>> CloseConversationAsync(string clientUserId, string otherUserId)
        {
            try
            {
                var allContracts = await _unitOfWork.Contracts.GetAllAsync();
                var hasCompletedContractAsClient = allContracts.Any(c =>
                    c.ClientId == clientUserId &&
                    c.FreelancerId == otherUserId &&
                    c.Status == ContractStatus.Completed);

                if (!hasCompletedContractAsClient)
                {
                    return ApiResponse<bool>.ErrorResponse("Only the client can close conversation after contract completion.");
                }

                var closureAlreadyExists = await _unitOfWork.Messages.AnyAsync(m =>
                    ((m.SenderId == clientUserId && m.ReceiverId == otherUserId) ||
                     (m.SenderId == otherUserId && m.ReceiverId == clientUserId)) &&
                    m.Subject == ClosedConversationSubject);

                if (closureAlreadyExists)
                {
                    return ApiResponse<bool>.SuccessResponse(true, "Conversation is already closed.");
                }

                await _unitOfWork.Messages.AddAsync(new Message
                {
                    SenderId = clientUserId,
                    ReceiverId = otherUserId,
                    Subject = ClosedConversationSubject,
                    Content = ClosedConversationContent,
                    IsRead = true
                });

                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Conversation closed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing conversation between {ClientUserId} and {OtherUserId}", clientUserId, otherUserId);
                return ApiResponse<bool>.ErrorResponse("Failed to close conversation.");
            }
        }

        public async Task<ApiResponse<bool>> DeleteMessageAsync(int messageId, string userId)
        {
            try
            {
                var message = await _unitOfWork.Messages.GetByIdAsync(messageId);
                if (message == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Message not found.");
                }

                if (message.SenderId != userId && message.ReceiverId != userId)
                {
                    return ApiResponse<bool>.ErrorResponse(Constants.ErrorMessages.Unauthorized);
                }

                message.IsDeleted = true;
                message.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Messages.Update(message);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Message deleted.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message {MessageId}", messageId);
                return ApiResponse<bool>.ErrorResponse("Failed to delete message.");
            }
        }

        public async Task<ApiResponse<bool>> DeleteConversationAsync(string userId, string otherUserId)
        {
            try
            {
                var conversationMessages = await _unitOfWork.Messages.FindAsync(m =>
                    (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                    (m.SenderId == otherUserId && m.ReceiverId == userId));

                var messagesToDelete = conversationMessages.ToList();
                if (!messagesToDelete.Any())
                {
                    return ApiResponse<bool>.SuccessResponse(true, "Conversation already empty.");
                }

                foreach (var message in messagesToDelete)
                {
                    message.IsDeleted = true;
                    message.UpdatedAt = DateTime.UtcNow;
                }

                _unitOfWork.Messages.UpdateRange(messagesToDelete);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Conversation deleted.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting conversation between {UserId} and {OtherUserId}", userId, otherUserId);
                return ApiResponse<bool>.ErrorResponse("Failed to delete conversation.");
            }
        }
    }
}
