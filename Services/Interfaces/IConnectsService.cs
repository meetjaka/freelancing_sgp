using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Utilities;

namespace SGP_Freelancing.Services.Interfaces
{
    public interface IConnectsService
    {
        Task<ConnectsWalletDto> GetWalletAsync(string userId);
        Task<ApiResponse<bool>> DeductConnectsAsync(string userId, int amount, string description, int? bidId = null);
        Task<ApiResponse<bool>> RefundConnectsAsync(string userId, int amount, string description);
        Task EnsureWalletExistsAsync(string userId);
        Task<int> GetBalanceAsync(string userId);
    }
}
