using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Utilities;

namespace SGP_Freelancing.Services.Interfaces
{
    public interface IMilestoneService
    {
        Task<ApiResponse<MilestoneDto>> CreateMilestoneAsync(CreateMilestoneDto dto, string userId);
        Task<ApiResponse<bool>> UpdateStatusAsync(int milestoneId, string newStatus, string userId);
        Task<List<MilestoneDto>> GetByContractAsync(int contractId);
        Task<ApiResponse<bool>> FundMilestoneAsync(int milestoneId, string clientId);
        Task<ApiResponse<bool>> SubmitMilestoneAsync(int milestoneId, string freelancerId);
        Task<ApiResponse<bool>> ApproveMilestoneAsync(int milestoneId, string clientId);
    }
}
