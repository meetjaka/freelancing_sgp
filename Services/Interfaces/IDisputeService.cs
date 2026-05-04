using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Utilities;

namespace SGP_Freelancing.Services.Interfaces
{
    public interface IDisputeService
    {
        Task<ApiResponse<DisputeDto>> RaiseDisputeAsync(CreateDisputeDto dto, string userId);
        Task<ApiResponse<bool>> ResolveDisputeAsync(ResolveDisputeDto dto, string adminId);
        Task<DisputeDto?> GetDisputeDetailsAsync(int id);
        Task<List<DisputeDto>> GetDisputesByContractAsync(int contractId);
        Task<List<DisputeDto>> GetAllPendingDisputesAsync();
    }
}
