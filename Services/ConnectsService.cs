using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SGP_Freelancing.Data;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Models.Entities;
using SGP_Freelancing.Services.Interfaces;
using SGP_Freelancing.Utilities;

namespace SGP_Freelancing.Services
{
    public class ConnectsService : IConnectsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ConnectsService> _logger;

        public ConnectsService(ApplicationDbContext context, IMapper mapper, ILogger<ConnectsService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task EnsureWalletExistsAsync(string userId)
        {
            var exists = await _context.ConnectsWallets.AnyAsync(w => w.UserId == userId);
            if (!exists)
            {
                var wallet = new ConnectsWallet
                {
                    UserId = userId,
                    Balance = 10,
                    MonthlyAllocation = 10,
                    LastRefillDate = DateTime.UtcNow
                };
                wallet.Transactions.Add(new ConnectsTransaction
                {
                    UserId = userId,
                    Amount = 10,
                    Type = ConnectsTransactionType.MonthlyRefill,
                    Description = "Welcome bonus: 10 free connects"
                });
                await _context.ConnectsWallets.AddAsync(wallet);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetBalanceAsync(string userId)
        {
            await EnsureWalletExistsAsync(userId);
            var wallet = await _context.ConnectsWallets.FirstOrDefaultAsync(w => w.UserId == userId);
            return wallet?.Balance ?? 0;
        }

        public async Task<ConnectsWalletDto> GetWalletAsync(string userId)
        {
            await EnsureWalletExistsAsync(userId);
            var wallet = await _context.ConnectsWallets
                .Include(w => w.Transactions.OrderByDescending(t => t.CreatedAt).Take(20))
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wallet == null)
                return new ConnectsWalletDto { Balance = 0, MonthlyAllocation = 10 };

            return new ConnectsWalletDto
            {
                Balance = wallet.Balance,
                MonthlyAllocation = wallet.MonthlyAllocation,
                LastRefillDate = wallet.LastRefillDate,
                RecentTransactions = _mapper.Map<List<ConnectsTransactionDto>>(wallet.Transactions)
            };
        }

        public async Task<ApiResponse<bool>> DeductConnectsAsync(string userId, int amount, string description, int? bidId = null)
        {
            await EnsureWalletExistsAsync(userId);
            var wallet = await _context.ConnectsWallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null) return ApiResponse<bool>.ErrorResponse("Wallet not found");
            if (wallet.Balance < amount) return ApiResponse<bool>.ErrorResponse($"Insufficient connects. You need {amount} but have {wallet.Balance}.");

            wallet.Balance -= amount;

            var tx = new ConnectsTransaction
            {
                UserId = userId,
                WalletId = wallet.Id,
                Amount = -amount,
                Type = ConnectsTransactionType.BidSpend,
                Description = description,
                BidId = bidId
            };

            await _context.ConnectsTransactions.AddAsync(tx);
            await _context.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true, $"{amount} connects deducted");
        }

        public async Task<ApiResponse<bool>> RefundConnectsAsync(string userId, int amount, string description)
        {
            await EnsureWalletExistsAsync(userId);
            var wallet = await _context.ConnectsWallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null) return ApiResponse<bool>.ErrorResponse("Wallet not found");

            wallet.Balance += amount;

            var tx = new ConnectsTransaction
            {
                UserId = userId,
                WalletId = wallet.Id,
                Amount = amount,
                Type = ConnectsTransactionType.Refund,
                Description = description
            };

            await _context.ConnectsTransactions.AddAsync(tx);
            await _context.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true, $"{amount} connects refunded");
        }
    }
}
