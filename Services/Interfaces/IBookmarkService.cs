using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Utilities;

namespace SGP_Freelancing.Services.Interfaces
{
    public interface IBookmarkService
    {
        Task<ApiResponse<BookmarkDto>> ToggleBookmarkAsync(CreateBookmarkDto dto, string userId);
        Task<ApiResponse<List<BookmarkDto>>> GetUserBookmarksAsync(string userId, string? bookmarkType = null);
        Task<ApiResponse<bool>> IsBookmarkedAsync(string userId, string bookmarkType, int itemId);
        Task<ApiResponse<List<int>>> GetBookmarkedItemIdsAsync(string userId, string bookmarkType);
        Task<ApiResponse<bool>> RemoveBookmarkAsync(int bookmarkId, string userId);
    }
}
