using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Utilities;

namespace SGP_Freelancing.Services.Interfaces
{
    public interface IReviewService
    {
        Task<ApiResponse<ReviewDto>> CreateReviewAsync(CreateReviewDto dto, string reviewerId);
        Task<List<ReviewDto>> GetReviewsByUserAsync(string userId);
        Task<List<ReviewDto>> GetReviewsByContractAsync(int contractId);
        Task<ReviewDto?> GetReviewAsync(int contractId, string reviewerId);
    }
}
