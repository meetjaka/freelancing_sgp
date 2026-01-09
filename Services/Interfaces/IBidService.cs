using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Utilities;

namespace SGP_Freelancing.Services.Interfaces
{
    public interface IBidService
    {
        Task<ApiResponse<BidDto>> CreateBidAsync(CreateBidDto dto, string freelancerId);
        Task<ApiResponse<BidDto>> AcceptBidAsync(int bidId, string clientId);
        Task<ApiResponse<BidDto>> RejectBidAsync(int bidId, string clientId);
        Task<ApiResponse<bool>> WithdrawBidAsync(int bidId, string freelancerId);
        Task<List<BidDto>> GetBidsByProjectAsync(int projectId);
        Task<List<BidDto>> GetBidsByFreelancerAsync(string freelancerId);
        Task<BidDto?> GetBidByIdAsync(int bidId);
    }
}
