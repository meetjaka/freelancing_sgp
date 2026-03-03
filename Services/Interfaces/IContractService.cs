using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Models.ViewModels;
using SGP_Freelancing.Utilities;

namespace SGP_Freelancing.Services.Interfaces
{
    public interface IContractService
    {
        Task<ApiResponse<ContractDto>> CreateContractAsync(CreateContractDto dto, string clientId);
        Task<ContractDetailsViewModel?> GetContractDetailsAsync(int id, string userId);
        Task<ApiResponse<bool>> CompleteContractAsync(int id, string userId);
        Task<ApiResponse<bool>> CancelContractAsync(int id, string userId);
        Task<List<ContractDto>> GetContractsByClientAsync(string clientId);
        Task<List<ContractDto>> GetContractsByFreelancerAsync(string freelancerId);
        Task<List<ContractDto>> GetActiveContractsAsync();
        Task<PagedResult<ContractDto>> GetContractsByClientPaginatedAsync(string clientId, int page = 1, int pageSize = 10);
        Task<PagedResult<ContractDto>> GetContractsByFreelancerPaginatedAsync(string freelancerId, int page = 1, int pageSize = 10);
    }
}
