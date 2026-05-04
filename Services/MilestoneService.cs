using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SGP_Freelancing.Data;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Models.Entities;
using SGP_Freelancing.Services.Interfaces;
using SGP_Freelancing.Utilities;

namespace SGP_Freelancing.Services
{
    public class MilestoneService : IMilestoneService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPaymentService _paymentService;
        private readonly ILogger<MilestoneService> _logger;

        public MilestoneService(ApplicationDbContext context, IMapper mapper, IPaymentService paymentService, ILogger<MilestoneService> logger)
        {
            _context = context;
            _mapper = mapper;
            _paymentService = paymentService;
            _logger = logger;
        }

        public async Task<ApiResponse<MilestoneDto>> CreateMilestoneAsync(CreateMilestoneDto dto, string userId)
        {
            try
            {
                var contract = await _context.Contracts.Include(c => c.Project).FirstOrDefaultAsync(c => c.Id == dto.ContractId);
                if (contract == null) return ApiResponse<MilestoneDto>.ErrorResponse("Contract not found");
                if (contract.ClientId != userId) return ApiResponse<MilestoneDto>.ErrorResponse("Only the client can add milestones");

                var milestone = _mapper.Map<Milestone>(dto);
                await _context.Milestones.AddAsync(milestone);
                await _context.SaveChangesAsync();

                var result = _mapper.Map<MilestoneDto>(milestone);
                return ApiResponse<MilestoneDto>.SuccessResponse(result, "Milestone created");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating milestone");
                return ApiResponse<MilestoneDto>.ErrorResponse("An error occurred");
            }
        }

        public async Task<List<MilestoneDto>> GetByContractAsync(int contractId)
        {
            var milestones = await _context.Milestones
                .Where(m => m.ContractId == contractId)
                .OrderBy(m => m.Order)
                .ToListAsync();
            return _mapper.Map<List<MilestoneDto>>(milestones);
        }

        public async Task<ApiResponse<bool>> FundMilestoneAsync(int milestoneId, string clientId)
        {
            var milestone = await _context.Milestones.Include(m => m.Contract).FirstOrDefaultAsync(m => m.Id == milestoneId);
            if (milestone == null) return ApiResponse<bool>.ErrorResponse("Milestone not found");
            if (milestone.Contract.ClientId != clientId) return ApiResponse<bool>.ErrorResponse("Unauthorized");
            if (milestone.Status != MilestoneStatus.Pending) return ApiResponse<bool>.ErrorResponse("Milestone is not in pending state");

            var escrowResult = await _paymentService.FundMilestoneEscrowAsync(milestoneId, milestone.ContractId, milestone.Amount, clientId);
            if (!escrowResult.Success) return escrowResult;

            milestone.Status = MilestoneStatus.Funded;
            await _context.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true, "Milestone funded and escrowed");
        }

        public async Task<ApiResponse<bool>> SubmitMilestoneAsync(int milestoneId, string freelancerId)
        {
            var milestone = await _context.Milestones.Include(m => m.Contract).FirstOrDefaultAsync(m => m.Id == milestoneId);
            if (milestone == null) return ApiResponse<bool>.ErrorResponse("Milestone not found");
            if (milestone.Contract.FreelancerId != freelancerId) return ApiResponse<bool>.ErrorResponse("Unauthorized");
            if (milestone.Status != MilestoneStatus.Funded && milestone.Status != MilestoneStatus.InProgress)
                return ApiResponse<bool>.ErrorResponse("Milestone must be funded first");

            milestone.Status = MilestoneStatus.Submitted;
            await _context.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true, "Milestone submitted for review");
        }

        public async Task<ApiResponse<bool>> ApproveMilestoneAsync(int milestoneId, string clientId)
        {
            var milestone = await _context.Milestones.Include(m => m.Contract).FirstOrDefaultAsync(m => m.Id == milestoneId);
            if (milestone == null) return ApiResponse<bool>.ErrorResponse("Milestone not found");
            if (milestone.Contract.ClientId != clientId) return ApiResponse<bool>.ErrorResponse("Unauthorized");
            if (milestone.Status != MilestoneStatus.Submitted) return ApiResponse<bool>.ErrorResponse("Milestone must be submitted first");

            var releaseResult = await _paymentService.ReleaseMilestoneEscrowAsync(milestoneId, milestone.ContractId, clientId);
            if (!releaseResult.Success) return releaseResult;

            milestone.Status = MilestoneStatus.Approved;
            await _context.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true, "Milestone approved, funds released");
        }

        public async Task<ApiResponse<bool>> UpdateStatusAsync(int milestoneId, string newStatus, string userId)
        {
            var milestone = await _context.Milestones.Include(m => m.Contract).FirstOrDefaultAsync(m => m.Id == milestoneId);
            if (milestone == null) return ApiResponse<bool>.ErrorResponse("Milestone not found");

            if (Enum.TryParse<MilestoneStatus>(newStatus, out var status))
            {
                milestone.Status = status;
                await _context.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResponse(true, "Status updated");
            }
            return ApiResponse<bool>.ErrorResponse("Invalid status");
        }
    }
}
