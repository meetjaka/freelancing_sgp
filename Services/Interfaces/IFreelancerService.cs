using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Utilities;

namespace SGP_Freelancing.Services.Interfaces
{
    public interface IFreelancerService
    {
        Task<ApiResponse<PagedResult<FreelancerProfileDto>>> SearchFreelancersAsync(FreelancerSearchDto searchDto);
        Task<ApiResponse<FreelancerProfileDto>> GetFreelancerDetailAsync(int profileId);
        Task<ApiResponse<List<FreelancerProfileDto>>> GetTopRatedFreelancersAsync(int count = 10);
    }
}
