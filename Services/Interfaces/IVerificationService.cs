using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Utilities;

namespace SGP_Freelancing.Services.Interfaces
{
    public interface IVerificationService
    {
        Task<ApiResponse<bool>> SubmitDocumentAsync(string userId, string documentUrl);
        Task<VerificationStatusDto> GetStatusAsync(string userId);
        Task<List<AdminVerificationDto>> GetPendingVerificationsAsync();
        Task<ApiResponse<bool>> ApproveVerificationAsync(string userId, string adminId, string? note);
        Task<ApiResponse<bool>> RejectVerificationAsync(string userId, string adminId, string note);
    }
}
