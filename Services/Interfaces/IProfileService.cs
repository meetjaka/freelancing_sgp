using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Utilities;

namespace SGP_Freelancing.Services.Interfaces
{
    public interface IProfileService
    {
        Task<ApiResponse<FreelancerProfileDto>> GetFreelancerProfileAsync(string userId);
        Task<ApiResponse<FreelancerProfileDto>> UpdateFreelancerProfileAsync(string userId, UpdateFreelancerProfileDto dto);
        
        Task<ApiResponse<ClientProfileDto>> GetClientProfileAsync(string userId);
        Task<ApiResponse<ClientProfileDto>> UpdateClientProfileAsync(string userId, UpdateClientProfileDto dto);
    }
}
