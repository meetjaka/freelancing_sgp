using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Models.ViewModels;
using SGP_Freelancing.Utilities;

namespace SGP_Freelancing.Services.Interfaces
{
    public interface IProjectService
    {
        Task<PagedResult<ProjectDto>> GetAllProjectsAsync(int pageNumber, int pageSize, int? categoryId = null, string? searchTerm = null);
        Task<ProjectDto?> GetProjectByIdAsync(int id);
        Task<ProjectDetailsViewModel?> GetProjectDetailsAsync(int id, string? currentUserId);
        Task<ApiResponse<ProjectDto>> CreateProjectAsync(CreateProjectDto dto, string clientId);
        Task<ApiResponse<ProjectDto>> UpdateProjectAsync(UpdateProjectDto dto, string userId);
        Task<ApiResponse<bool>> DeleteProjectAsync(int id, string userId);
        Task<ApiResponse<bool>> CloseProjectAsync(int id, string userId);
        Task<List<ProjectDto>> GetProjectsByClientAsync(string clientId);
        Task<List<ProjectDto>> GetRecommendedProjectsAsync(string freelancerId, int count = 10);
    }
}
