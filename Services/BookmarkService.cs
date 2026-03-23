using Microsoft.EntityFrameworkCore;
using SGP_Freelancing.Data;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Models.Entities;
using SGP_Freelancing.Services.Interfaces;
using SGP_Freelancing.Utilities;

namespace SGP_Freelancing.Services
{
    public class BookmarkService : IBookmarkService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BookmarkService> _logger;

        public BookmarkService(ApplicationDbContext context, ILogger<BookmarkService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<BookmarkDto>> ToggleBookmarkAsync(CreateBookmarkDto dto, string userId)
        {
            try
            {
                // Check if already bookmarked
                var existing = await _context.Bookmarks
                    .FirstOrDefaultAsync(b => b.UserId == userId 
                        && b.BookmarkType == dto.BookmarkType 
                        && b.ItemId == dto.ItemId);

                if (existing != null)
                {
                    // Remove bookmark (toggle off)
                    existing.IsDeleted = true;
                    existing.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return ApiResponse<BookmarkDto>.SuccessResponse(null!, "Bookmark removed");
                }

                // Validate the item exists
                if (dto.BookmarkType == BookmarkTypes.Project)
                {
                    var project = await _context.Projects.FindAsync(dto.ItemId);
                    if (project == null)
                        return ApiResponse<BookmarkDto>.ErrorResponse("Project not found");
                }
                else if (dto.BookmarkType == BookmarkTypes.Freelancer)
                {
                    var profile = await _context.FreelancerProfiles.FindAsync(dto.ItemId);
                    if (profile == null)
                        return ApiResponse<BookmarkDto>.ErrorResponse("Freelancer not found");
                }
                else
                {
                    return ApiResponse<BookmarkDto>.ErrorResponse("Invalid bookmark type");
                }

                // Create bookmark
                var bookmark = new Bookmark
                {
                    UserId = userId,
                    BookmarkType = dto.BookmarkType,
                    ItemId = dto.ItemId,
                    Note = dto.Note
                };

                await _context.Bookmarks.AddAsync(bookmark);
                await _context.SaveChangesAsync();

                var bookmarkDto = new BookmarkDto
                {
                    Id = bookmark.Id,
                    BookmarkType = bookmark.BookmarkType,
                    ItemId = bookmark.ItemId,
                    Note = bookmark.Note,
                    CreatedAt = bookmark.CreatedAt
                };

                return ApiResponse<BookmarkDto>.SuccessResponse(bookmarkDto, "Bookmark added");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling bookmark");
                return ApiResponse<BookmarkDto>.ErrorResponse("Failed to toggle bookmark");
            }
        }

        public async Task<ApiResponse<List<BookmarkDto>>> GetUserBookmarksAsync(string userId, string? bookmarkType = null)
        {
            try
            {
                var query = _context.Bookmarks
                    .Where(b => b.UserId == userId);

                if (!string.IsNullOrEmpty(bookmarkType))
                    query = query.Where(b => b.BookmarkType == bookmarkType);

                var bookmarks = await query
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync();

                var bookmarkDtos = new List<BookmarkDto>();

                foreach (var bookmark in bookmarks)
                {
                    var dto = new BookmarkDto
                    {
                        Id = bookmark.Id,
                        BookmarkType = bookmark.BookmarkType,
                        ItemId = bookmark.ItemId,
                        Note = bookmark.Note,
                        CreatedAt = bookmark.CreatedAt
                    };

                    // Populate item details based on type
                    if (bookmark.BookmarkType == BookmarkTypes.Project)
                    {
                        var project = await _context.Projects
                            .Include(p => p.Category)
                            .Include(p => p.Client)
                            .Include(p => p.Bids)
                            .Include(p => p.ProjectSkills)
                                .ThenInclude(ps => ps.Skill)
                            .FirstOrDefaultAsync(p => p.Id == bookmark.ItemId);

                        if (project != null)
                        {
                            dto.ItemTitle = project.Title;
                            dto.ItemDescription = project.Description.Length > 200 
                                ? project.Description.Substring(0, 200) + "..." 
                                : project.Description;
                            dto.ItemBudgetOrRate = project.Budget;
                            dto.ItemCategory = project.Category?.Name;
                            dto.ItemOwnerName = project.Client?.FirstName + " " + project.Client?.LastName;
                            dto.ItemBidsCount = project.Bids.Count;
                            dto.ItemSkills = project.ProjectSkills.Select(ps => ps.Skill.Name).ToList();
                        }
                        else
                        {
                            // Project was deleted, skip
                            continue;
                        }
                    }
                    else if (bookmark.BookmarkType == BookmarkTypes.Freelancer)
                    {
                        var profile = await _context.FreelancerProfiles
                            .Include(fp => fp.User)
                            .Include(fp => fp.FreelancerSkills)
                                .ThenInclude(fs => fs.Skill)
                            .FirstOrDefaultAsync(fp => fp.Id == bookmark.ItemId);

                        if (profile != null)
                        {
                            dto.ItemTitle = profile.Title ?? profile.User?.FirstName + " " + profile.User?.LastName;
                            dto.ItemDescription = profile.Bio?.Length > 200 
                                ? profile.Bio.Substring(0, 200) + "..." 
                                : profile.Bio;
                            dto.ItemBudgetOrRate = profile.HourlyRate;
                            dto.ItemRating = profile.AverageRating;
                            dto.ItemOwnerName = profile.User?.FirstName + " " + profile.User?.LastName;
                            dto.ItemSkills = profile.FreelancerSkills.Select(fs => fs.Skill.Name).ToList();
                        }
                        else
                        {
                            continue;
                        }
                    }

                    bookmarkDtos.Add(dto);
                }

                return ApiResponse<List<BookmarkDto>>.SuccessResponse(bookmarkDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user bookmarks");
                return ApiResponse<List<BookmarkDto>>.ErrorResponse("Failed to retrieve bookmarks");
            }
        }

        public async Task<ApiResponse<bool>> IsBookmarkedAsync(string userId, string bookmarkType, int itemId)
        {
            try
            {
                var exists = await _context.Bookmarks
                    .AnyAsync(b => b.UserId == userId && b.BookmarkType == bookmarkType && b.ItemId == itemId);
                return ApiResponse<bool>.SuccessResponse(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking bookmark status");
                return ApiResponse<bool>.ErrorResponse("Failed to check bookmark");
            }
        }

        public async Task<ApiResponse<List<int>>> GetBookmarkedItemIdsAsync(string userId, string bookmarkType)
        {
            try
            {
                var ids = await _context.Bookmarks
                    .Where(b => b.UserId == userId && b.BookmarkType == bookmarkType)
                    .Select(b => b.ItemId)
                    .ToListAsync();
                return ApiResponse<List<int>>.SuccessResponse(ids);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookmarked item IDs");
                return ApiResponse<List<int>>.ErrorResponse("Failed to get bookmarks");
            }
        }

        public async Task<ApiResponse<bool>> RemoveBookmarkAsync(int bookmarkId, string userId)
        {
            try
            {
                var bookmark = await _context.Bookmarks
                    .FirstOrDefaultAsync(b => b.Id == bookmarkId && b.UserId == userId);

                if (bookmark == null)
                    return ApiResponse<bool>.ErrorResponse("Bookmark not found");

                bookmark.IsDeleted = true;
                bookmark.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Bookmark removed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing bookmark");
                return ApiResponse<bool>.ErrorResponse("Failed to remove bookmark");
            }
        }
    }
}
