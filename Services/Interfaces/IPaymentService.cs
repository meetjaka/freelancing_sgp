using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Utilities;

namespace SGP_Freelancing.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<ApiResponse<bool>> FundMilestoneEscrowAsync(int milestoneId, int contractId, decimal amount, string clientId);
        Task<ApiResponse<bool>> ReleaseMilestoneEscrowAsync(int milestoneId, int contractId, string clientId);
        Task<decimal> GetEscrowBalanceAsync(int contractId);
        Task<decimal> GetFreelancerEarningsAsync(string freelancerId);
    }
}
