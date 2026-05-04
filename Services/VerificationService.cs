using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SGP_Freelancing.Data;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Models.Entities;
using SGP_Freelancing.Services.Interfaces;
using SGP_Freelancing.Utilities;

namespace SGP_Freelancing.Services
{
    public class VerificationService : IVerificationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<VerificationService> _logger;

        public VerificationService(UserManager<ApplicationUser> userManager, ILogger<VerificationService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> SubmitDocumentAsync(string userId, string documentUrl)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return ApiResponse<bool>.ErrorResponse("User not found");

            user.VerificationDocumentUrl = documentUrl;
            user.VerificationStatus = VerificationStatus.Pending;
            user.VerificationSubmittedAt = DateTime.UtcNow;
            user.VerificationNote = null;

            await _userManager.UpdateAsync(user);
            return ApiResponse<bool>.SuccessResponse(true, "Document submitted for review");
        }

        public async Task<VerificationStatusDto> GetStatusAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return new VerificationStatusDto { Status = "None" };

            return new VerificationStatusDto
            {
                IsVerified = user.IsVerified,
                Status = user.VerificationStatus.ToString(),
                DocumentUrl = user.VerificationDocumentUrl,
                SubmittedAt = user.VerificationSubmittedAt,
                ReviewedAt = user.VerificationReviewedAt,
                Note = user.VerificationNote
            };
        }

        public async Task<List<AdminVerificationDto>> GetPendingVerificationsAsync()
        {
            var users = _userManager.Users
                .Where(u => u.VerificationStatus == VerificationStatus.Pending)
                .ToList();

            return users.Select(u => new AdminVerificationDto
            {
                UserId = u.Id,
                UserName = u.FirstName + " " + u.LastName,
                UserEmail = u.Email,
                DocumentUrl = u.VerificationDocumentUrl,
                Status = u.VerificationStatus.ToString(),
                SubmittedAt = u.VerificationSubmittedAt
            }).ToList();
        }

        public async Task<ApiResponse<bool>> ApproveVerificationAsync(string userId, string adminId, string? note)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return ApiResponse<bool>.ErrorResponse("User not found");

            user.VerificationStatus = VerificationStatus.Approved;
            user.IsVerified = true;
            user.VerificationReviewedAt = DateTime.UtcNow;
            user.VerificationNote = note ?? "Verified by admin";

            await _userManager.UpdateAsync(user);
            _logger.LogInformation("User {UserId} verified by admin {AdminId}", userId, adminId);
            return ApiResponse<bool>.SuccessResponse(true, "User verified successfully");
        }

        public async Task<ApiResponse<bool>> RejectVerificationAsync(string userId, string adminId, string note)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return ApiResponse<bool>.ErrorResponse("User not found");

            user.VerificationStatus = VerificationStatus.Rejected;
            user.IsVerified = false;
            user.VerificationReviewedAt = DateTime.UtcNow;
            user.VerificationNote = note;

            await _userManager.UpdateAsync(user);
            return ApiResponse<bool>.SuccessResponse(true, "Verification rejected");
        }
    }
}
