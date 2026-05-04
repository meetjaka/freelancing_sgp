using Microsoft.EntityFrameworkCore;
using SGP_Freelancing.Data;
using SGP_Freelancing.Models.Entities;
using SGP_Freelancing.Services.Interfaces;
using SGP_Freelancing.Utilities;

namespace SGP_Freelancing.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(ApplicationDbContext context, ILogger<PaymentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> FundMilestoneEscrowAsync(int milestoneId, int contractId, decimal amount, string clientId)
        {
            try
            {
                var transaction = new PaymentTransaction
                {
                    ContractId = contractId,
                    MilestoneId = milestoneId,
                    Amount = amount,
                    Status = PaymentStatus.Escrow,
                    Type = PaymentType.EscrowDeposit,
                    Description = $"Escrow deposit for milestone #{milestoneId}",
                    ProcessedAt = DateTime.UtcNow
                };

                await _context.PaymentTransactions.AddAsync(transaction);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Escrow funded: {Amount} for milestone {MilestoneId}", amount, milestoneId);
                return ApiResponse<bool>.SuccessResponse(true, "Escrow funded");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error funding escrow");
                return ApiResponse<bool>.ErrorResponse("Error funding escrow");
            }
        }

        public async Task<ApiResponse<bool>> ReleaseMilestoneEscrowAsync(int milestoneId, int contractId, string clientId)
        {
            try
            {
                var escrowTx = await _context.PaymentTransactions
                    .FirstOrDefaultAsync(pt => pt.MilestoneId == milestoneId && pt.Status == PaymentStatus.Escrow);

                if (escrowTx == null) return ApiResponse<bool>.ErrorResponse("No escrowed funds found");

                escrowTx.Status = PaymentStatus.Released;

                var releaseTx = new PaymentTransaction
                {
                    ContractId = contractId,
                    MilestoneId = milestoneId,
                    Amount = escrowTx.Amount,
                    Status = PaymentStatus.Completed,
                    Type = PaymentType.EscrowRelease,
                    Description = $"Escrow released for milestone #{milestoneId}",
                    ProcessedAt = DateTime.UtcNow
                };

                await _context.PaymentTransactions.AddAsync(releaseTx);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Escrow released for milestone {MilestoneId}", milestoneId);
                return ApiResponse<bool>.SuccessResponse(true, "Funds released to freelancer");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing escrow");
                return ApiResponse<bool>.ErrorResponse("Error releasing escrow");
            }
        }

        public async Task<decimal> GetEscrowBalanceAsync(int contractId)
        {
            return await _context.PaymentTransactions
                .Where(pt => pt.ContractId == contractId && pt.Status == PaymentStatus.Escrow)
                .SumAsync(pt => pt.Amount);
        }

        public async Task<decimal> GetFreelancerEarningsAsync(string freelancerId)
        {
            var contractIds = await _context.Contracts
                .Where(c => c.FreelancerId == freelancerId)
                .Select(c => c.Id)
                .ToListAsync();

            return await _context.PaymentTransactions
                .Where(pt => contractIds.Contains(pt.ContractId) && pt.Type == PaymentType.EscrowRelease && pt.Status == PaymentStatus.Completed)
                .SumAsync(pt => pt.Amount);
        }
    }
}
