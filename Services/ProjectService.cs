using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Models.Entities;
using SGP_Freelancing.Models.ViewModels;
using SGP_Freelancing.Repositories;
using SGP_Freelancing.Services.Interfaces;
using SGP_Freelancing.Utilities;

namespace SGP_Freelancing.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ProjectService> _logger;

        public ProjectService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ProjectService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<ProjectDto>> GetAllProjectsAsync(int pageNumber, int pageSize, int? categoryId = null, string? searchTerm = null)
        {
            try
            {
                var query = _unitOfWork.Projects.Query()
                    .Include(p => p.Category)
                    .Include(p => p.Client)
                    .Include(p => p.Bids)
                    .Include(p => p.ProjectSkills)
                        .ThenInclude(ps => ps.Skill)
                    .Where(p => p.Status == ProjectStatus.Open);

                if (categoryId.HasValue)
                    query = query.Where(p => p.CategoryId == categoryId.Value);

                if (!string.IsNullOrWhiteSpace(searchTerm))
                    query = query.Where(p => p.Title.Contains(searchTerm) || p.Description.Contains(searchTerm));

                var totalCount = await query.CountAsync();
                var projects = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var projectDtos = _mapper.Map<List<ProjectDto>>(projects);

                return new PagedResult<ProjectDto>
                {
                    Items = projectDtos,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting projects");
                throw;
            }
        }

        public async Task<ProjectDto?> GetProjectByIdAsync(int id)
        {
            var project = await _unitOfWork.Projects.GetProjectWithBidsAsync(id);
            return project != null ? _mapper.Map<ProjectDto>(project) : null;
        }

        public async Task<ProjectDetailsViewModel?> GetProjectDetailsAsync(int id, string? currentUserId)
        {
            var project = await _unitOfWork.Projects.GetProjectWithBidsAsync(id);
            if (project == null) return null;

            var viewModel = new ProjectDetailsViewModel
            {
                Project = _mapper.Map<ProjectDto>(project),
                Bids = _mapper.Map<List<BidDto>>(project.Bids),
                IsOwner = project.ClientId == currentUserId,
                CanBid = !string.IsNullOrEmpty(currentUserId) && project.ClientId != currentUserId
            };

            return viewModel;
        }

        public async Task<ApiResponse<ProjectDto>> CreateProjectAsync(CreateProjectDto dto, string clientId)
        {
            try
            {
                var project = _mapper.Map<Project>(dto);
                project.ClientId = clientId;
                project.Status = ProjectStatus.Open;

                await _unitOfWork.Projects.AddAsync(project);

                // Add skills if provided
                if (dto.SkillIds != null && dto.SkillIds.Any())
                {
                    foreach (var skillId in dto.SkillIds)
                    {
                        var projectSkill = new ProjectSkill
                        {
                            ProjectId = project.Id,
                            SkillId = skillId
                        };
                        await _unitOfWork.Repository<ProjectSkill>().AddAsync(projectSkill);
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                var projectDto = await GetProjectByIdAsync(project.Id);
                return ApiResponse<ProjectDto>.SuccessResponse(projectDto!, Constants.Messages.ProjectCreated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project");
                return ApiResponse<ProjectDto>.ErrorResponse("Failed to create project");
            }
        }

        public async Task<ApiResponse<ProjectDto>> UpdateProjectAsync(UpdateProjectDto dto, string userId)
        {
            try
            {
                var project = await _unitOfWork.Projects.GetByIdAsync(dto.Id);
                if (project == null)
                    return ApiResponse<ProjectDto>.ErrorResponse(Constants.ErrorMessages.ProjectNotFound);

                if (project.ClientId != userId)
                    return ApiResponse<ProjectDto>.ErrorResponse(Constants.ErrorMessages.Unauthorized);

                project.Title = dto.Title;
                project.Description = dto.Description;
                project.Budget = dto.Budget;
                project.Deadline = dto.Deadline;
                project.CategoryId = dto.CategoryId;
                project.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Projects.Update(project);
                await _unitOfWork.SaveChangesAsync();

                var projectDto = await GetProjectByIdAsync(project.Id);
                return ApiResponse<ProjectDto>.SuccessResponse(projectDto!, "Project updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating project");
                return ApiResponse<ProjectDto>.ErrorResponse("Failed to update project");
            }
        }

        public async Task<ApiResponse<bool>> DeleteProjectAsync(int id, string userId)
        {
            try
            {
                var project = await _unitOfWork.Projects.GetByIdAsync(id);
                if (project == null)
                    return ApiResponse<bool>.ErrorResponse(Constants.ErrorMessages.ProjectNotFound);

                if (project.ClientId != userId)
                    return ApiResponse<bool>.ErrorResponse(Constants.ErrorMessages.Unauthorized);

                // Soft delete
                project.IsDeleted = true;
                project.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Projects.Update(project);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Project deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting project");
                return ApiResponse<bool>.ErrorResponse("Failed to delete project");
            }
        }

        public async Task<ApiResponse<bool>> CloseProjectAsync(int id, string userId)
        {
            try
            {
                var project = await _unitOfWork.Projects.GetByIdAsync(id);
                if (project == null)
                    return ApiResponse<bool>.ErrorResponse(Constants.ErrorMessages.ProjectNotFound);

                if (project.ClientId != userId)
                    return ApiResponse<bool>.ErrorResponse(Constants.ErrorMessages.Unauthorized);

                project.Status = ProjectStatus.Closed;
                project.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Projects.Update(project);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Project closed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing project");
                return ApiResponse<bool>.ErrorResponse("Failed to close project");
            }
        }

        public async Task<List<ProjectDto>> GetProjectsByClientAsync(string clientId)
        {
            var projects = await _unitOfWork.Projects.GetProjectsByClientAsync(clientId);
            return _mapper.Map<List<ProjectDto>>(projects);
        }

        public async Task<List<ProjectDto>> GetRecommendedProjectsAsync(string freelancerId, int count = 10)
        {
            // Get freelancer skills
            var freelancerProfile = await _unitOfWork.FreelancerProfiles.GetByUserIdAsync(freelancerId);
            if (freelancerProfile == null)
                return new List<ProjectDto>();

            var freelancerSkillIds = freelancerProfile.FreelancerSkills.Select(fs => fs.SkillId).ToList();

            // Find projects matching skills
            var projects = await _unitOfWork.Projects.Query()
                .Include(p => p.Category)
                .Include(p => p.Client)
                .Include(p => p.ProjectSkills)
                    .ThenInclude(ps => ps.Skill)
                .Where(p => p.Status == ProjectStatus.Open && 
                           p.ProjectSkills.Any(ps => freelancerSkillIds.Contains(ps.SkillId)))
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();

            return _mapper.Map<List<ProjectDto>>(projects);
        }
    }
}
