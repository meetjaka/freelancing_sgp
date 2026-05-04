using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SGP_Freelancing.Data;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Models.Entities;
using SGP_Freelancing.Services.Interfaces;
using SGP_Freelancing.Utilities;

namespace SGP_Freelancing.Services
{
    public class TimeTrackingService : ITimeTrackingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<TimeTrackingService> _logger;

        public TimeTrackingService(ApplicationDbContext context, IMapper mapper, ILogger<TimeTrackingService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<TimeEntryDto>> LogHoursAsync(CreateTimeEntryDto dto, string freelancerId)
        {
            try
            {
                var contract = await _context.Contracts.FirstOrDefaultAsync(c => c.Id == dto.ContractId);
                if (contract == null) return ApiResponse<TimeEntryDto>.ErrorResponse("Contract not found");
                if (contract.FreelancerId != freelancerId) return ApiResponse<TimeEntryDto>.ErrorResponse("Unauthorized");
                if (contract.Status != ContractStatus.Active) return ApiResponse<TimeEntryDto>.ErrorResponse("Contract is not active");

                var entry = _mapper.Map<TimeEntry>(dto);
                entry.FreelancerId = freelancerId;
                entry.Status = TimeEntryStatus.Logged;

                await _context.TimeEntries.AddAsync(entry);
                await _context.SaveChangesAsync();

                var full = await _context.TimeEntries.Include(t => t.Freelancer).FirstAsync(t => t.Id == entry.Id);
                return ApiResponse<TimeEntryDto>.SuccessResponse(_mapper.Map<TimeEntryDto>(full), "Hours logged");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging hours");
                return ApiResponse<TimeEntryDto>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<bool>> ApproveEntryAsync(int entryId, string clientId)
        {
            var entry = await _context.TimeEntries.Include(t => t.Contract).FirstOrDefaultAsync(t => t.Id == entryId);
            if (entry == null) return ApiResponse<bool>.ErrorResponse("Entry not found");
            if (entry.Contract.ClientId != clientId) return ApiResponse<bool>.ErrorResponse("Unauthorized");

            entry.Status = TimeEntryStatus.Approved;
            entry.ApprovedByClientId = clientId;
            entry.ApprovedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true, "Time entry approved");
        }

        public async Task<ApiResponse<bool>> RejectEntryAsync(int entryId, string clientId)
        {
            var entry = await _context.TimeEntries.Include(t => t.Contract).FirstOrDefaultAsync(t => t.Id == entryId);
            if (entry == null) return ApiResponse<bool>.ErrorResponse("Entry not found");
            if (entry.Contract.ClientId != clientId) return ApiResponse<bool>.ErrorResponse("Unauthorized");

            entry.Status = TimeEntryStatus.Rejected;
            entry.ApprovedByClientId = clientId;
            entry.ApprovedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true, "Time entry rejected");
        }

        public async Task<TimesheetSummaryDto> GetTimesheetAsync(int contractId)
        {
            var contract = await _context.Contracts
                .Include(c => c.Project)
                .Include(c => c.Freelancer).ThenInclude(f => f.FreelancerProfile)
                .FirstOrDefaultAsync(c => c.Id == contractId);

            var entries = await _context.TimeEntries
                .Where(t => t.ContractId == contractId)
                .Include(t => t.Freelancer)
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            var hourlyRate = contract?.Freelancer?.FreelancerProfile?.HourlyRate ?? 0;
            var approvedHours = entries.Where(e => e.Status == TimeEntryStatus.Approved).Sum(e => e.HoursWorked);

            return new TimesheetSummaryDto
            {
                ContractId = contractId,
                ProjectTitle = contract?.Project?.Title ?? "Unknown",
                TotalHours = entries.Sum(e => e.HoursWorked),
                ApprovedHours = approvedHours,
                PendingHours = entries.Where(e => e.Status == TimeEntryStatus.Logged).Sum(e => e.HoursWorked),
                HourlyRate = hourlyRate,
                TotalEarnings = approvedHours * hourlyRate,
                Entries = _mapper.Map<List<TimeEntryDto>>(entries)
            };
        }
    }
}
