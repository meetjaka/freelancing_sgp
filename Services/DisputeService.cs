using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SGP_Freelancing.Data;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Models.Entities;
using SGP_Freelancing.Services.Interfaces;
using SGP_Freelancing.Utilities;

namespace SGP_Freelancing.Services
{
    public class DisputeService : IDisputeService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<DisputeService> _logger;

        public DisputeService(ApplicationDbContext context, IMapper mapper, ILogger<DisputeService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<DisputeDto>> RaiseDisputeAsync(CreateDisputeDto dto, string userId)
        {
            try
            {
                var contract = await _context.Contracts.FirstOrDefaultAsync(c => c.Id == dto.ContractId);
                if (contract == null) return ApiResponse<DisputeDto>.ErrorResponse("Contract not found");
                if (contract.ClientId != userId && contract.FreelancerId != userId)
                    return ApiResponse<DisputeDto>.ErrorResponse("You are not part of this contract");

                var dispute = new Dispute
                {
                    ContractId = dto.ContractId,
                    RaisedByUserId = userId,
                    Reason = dto.Reason,
                    Status = DisputeStatus.Open
                };

                contract.Status = ContractStatus.Disputed;

                await _context.Disputes.AddAsync(dispute);
                await _context.SaveChangesAsync();

                var full = await GetDisputeDetailsAsync(dispute.Id);
                return ApiResponse<DisputeDto>.SuccessResponse(full!, "Dispute raised successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error raising dispute");
                return ApiResponse<DisputeDto>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<bool>> ResolveDisputeAsync(ResolveDisputeDto dto, string adminId)
        {
            var dispute = await _context.Disputes.Include(d => d.Contract).FirstOrDefaultAsync(d => d.Id == dto.DisputeId);
            if (dispute == null) return ApiResponse<bool>.ErrorResponse("Dispute not found");

            dispute.Status = DisputeStatus.Resolved;
            dispute.Resolution = dto.Resolution;
            dispute.ResolvedByAdminId = adminId;
            dispute.ResolvedAt = DateTime.UtcNow;

            dispute.Contract.Status = ContractStatus.Active;

            await _context.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true, "Dispute resolved");
        }

        public async Task<DisputeDto?> GetDisputeDetailsAsync(int id)
        {
            var dispute = await _context.Disputes
                .Include(d => d.RaisedByUser)
                .Include(d => d.ResolvedByAdmin)
                .Include(d => d.Contract).ThenInclude(c => c.Project)
                .Include(d => d.Contract).ThenInclude(c => c.Client)
                .Include(d => d.Contract).ThenInclude(c => c.Freelancer)
                .FirstOrDefaultAsync(d => d.Id == id);
            return dispute == null ? null : _mapper.Map<DisputeDto>(dispute);
        }

        public async Task<List<DisputeDto>> GetDisputesByContractAsync(int contractId)
        {
            var disputes = await _context.Disputes
                .Where(d => d.ContractId == contractId)
                .Include(d => d.RaisedByUser)
                .Include(d => d.Contract).ThenInclude(c => c.Project)
                .Include(d => d.Contract).ThenInclude(c => c.Client)
                .Include(d => d.Contract).ThenInclude(c => c.Freelancer)
                .OrderByDescending(d => d.CreatedAt).ToListAsync();
            return _mapper.Map<List<DisputeDto>>(disputes);
        }

        public async Task<List<DisputeDto>> GetAllPendingDisputesAsync()
        {
            var disputes = await _context.Disputes
                .Where(d => d.Status == DisputeStatus.Open || d.Status == DisputeStatus.UnderReview)
                .Include(d => d.RaisedByUser)
                .Include(d => d.ResolvedByAdmin)
                .Include(d => d.Contract).ThenInclude(c => c.Project)
                .Include(d => d.Contract).ThenInclude(c => c.Client)
                .Include(d => d.Contract).ThenInclude(c => c.Freelancer)
                .OrderByDescending(d => d.CreatedAt).ToListAsync();
            return _mapper.Map<List<DisputeDto>>(disputes);
        }
    }
}
