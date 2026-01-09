using AutoMapper;
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
                var message = _mapper.Map<Message>(dto);
                message.SenderId = senderId;
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
            var unreadCount = await _unitOfWork.Messages.GetUnreadCountAsync(userId);

            return new MessagesViewModel
            {
                Messages = _mapper.Map<List<MessageDto>>(messages),
                UnreadCount = unreadCount
            };
        }

        public async Task<ConversationViewModel> GetConversationAsync(string userId, string otherUserId)
        {
            var messages = await _unitOfWork.Messages.GetConversationAsync(userId, otherUserId);
            var otherUser = await _unitOfWork.Repository<ApplicationUser>().FirstOrDefaultAsync(u => u.Id == otherUserId);

            return new ConversationViewModel
            {
                Messages = _mapper.Map<List<MessageDto>>(messages),
                OtherUserId = otherUserId,
                OtherUserName = otherUser != null ? $"{otherUser.FirstName} {otherUser.LastName}" : "Unknown"
            };
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
    }
}
