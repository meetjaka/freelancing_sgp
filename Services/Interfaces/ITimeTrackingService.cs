using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Utilities;

namespace SGP_Freelancing.Services.Interfaces
{
    public interface ITimeTrackingService
    {
        Task<ApiResponse<TimeEntryDto>> LogHoursAsync(CreateTimeEntryDto dto, string freelancerId);
        Task<ApiResponse<bool>> ApproveEntryAsync(int entryId, string clientId);
        Task<ApiResponse<bool>> RejectEntryAsync(int entryId, string clientId);
        Task<TimesheetSummaryDto> GetTimesheetAsync(int contractId);
    }
}
