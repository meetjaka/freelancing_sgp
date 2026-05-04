using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Utilities;

namespace SGP_Freelancing.Services.Interfaces
{
    public interface IGigService
    {
        Task<PagedResult<FreelancerServiceDto>> GetAllServicesAsync(string? search, int? categoryId, int page = 1, int pageSize = 12);
        Task<FreelancerServiceDto?> GetServiceDetailsAsync(int id);
        Task<ApiResponse<FreelancerServiceDto>> CreateServiceAsync(CreateFreelancerServiceDto dto, string freelancerId);
        Task<ApiResponse<bool>> UpdateServiceAsync(int id, CreateFreelancerServiceDto dto, string freelancerId);
        Task<ApiResponse<bool>> DeleteServiceAsync(int id, string freelancerId);
        Task<List<FreelancerServiceDto>> GetMyServicesAsync(string freelancerId);
        Task<ApiResponse<ServiceOrderDto>> PlaceOrderAsync(CreateServiceOrderDto dto, string clientId);
        Task<ApiResponse<bool>> UpdateOrderStatusAsync(int orderId, string newStatus, string userId);
        Task<List<ServiceOrderDto>> GetClientOrdersAsync(string clientId);
        Task<List<ServiceOrderDto>> GetFreelancerOrdersAsync(string freelancerId);
    }
}
