using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Models.ViewModels;
using SGP_Freelancing.Utilities;

namespace SGP_Freelancing.Services.Interfaces
{
    public interface IMessageService
    {
        Task<ApiResponse<MessageDto>> SendMessageAsync(SendMessageDto dto, string senderId);
        Task<MessagesViewModel> GetUserMessagesAsync(string userId);
        Task<ConversationViewModel> GetConversationAsync(string userId, string otherUserId);
        Task<PagedResult<MessageDto>> GetConversationPaginatedAsync(string userId, string otherUserId, int page = 1, int pageSize = 20);
        Task<ApiResponse<bool>> MarkAsReadAsync(int messageId, string userId);
        Task<int> GetUnreadCountAsync(string userId);
        Task<ApiResponse<bool>> CanMessageAsync(string userId, string otherUserId);
        Task<ApiResponse<bool>> CloseConversationAsync(string clientUserId, string otherUserId);
        Task<ApiResponse<bool>> DeleteMessageAsync(int messageId, string userId);
        Task<ApiResponse<bool>> DeleteConversationAsync(string userId, string otherUserId);
    }
}
