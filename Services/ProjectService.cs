using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Models.Entities;
using SGP_Freelancing.Models.ViewModels;
using SGP_Freelancing.Repositories;
using SGP_Freelancing.Services.Interfaces;
using SGP_Freelancing.Utilities;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace SGP_Freelancing.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ProjectService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ProjectService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ProjectService> logger, HttpClient httpClient, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _httpClient = httpClient;
            _configuration = configuration;
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

        public async Task<PagedResult<ProjectDto>> AdvancedSearchAsync(ProjectSearchDto searchDto)
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

                // Text search
                if (!string.IsNullOrWhiteSpace(searchDto.SearchTerm))
                {
                    var term = searchDto.SearchTerm.ToLower();
                    query = query.Where(p => p.Title.ToLower().Contains(term) 
                        || p.Description.ToLower().Contains(term));
                }

                // Category filter
                if (searchDto.CategoryId.HasValue)
                    query = query.Where(p => p.CategoryId == searchDto.CategoryId.Value);

                // Budget range filter
                if (searchDto.MinBudget.HasValue)
                    query = query.Where(p => p.Budget >= searchDto.MinBudget.Value);

                if (searchDto.MaxBudget.HasValue)
                    query = query.Where(p => p.Budget <= searchDto.MaxBudget.Value);

                // Skill-based filtering
                if (searchDto.SkillIds != null && searchDto.SkillIds.Any())
                {
                    query = query.Where(p => p.ProjectSkills.Any(ps => searchDto.SkillIds.Contains(ps.SkillId)));
                }

                // Deadline filter (projects due within X days)
                if (searchDto.DeadlineWithinDays.HasValue)
                {
                    var deadlineLimit = DateTime.UtcNow.AddDays(searchDto.DeadlineWithinDays.Value);
                    query = query.Where(p => p.Deadline.HasValue && p.Deadline.Value <= deadlineLimit);
                }

                var totalCount = await query.CountAsync();

                // Sorting
                query = searchDto.SortBy?.ToLower() switch
                {
                    "oldest" => query.OrderBy(p => p.CreatedAt),
                    "budget-asc" => query.OrderBy(p => p.Budget),
                    "budget-desc" => query.OrderByDescending(p => p.Budget),
                    "most-bids" => query.OrderByDescending(p => p.Bids.Count),
                    "deadline" => query.OrderBy(p => p.Deadline),
                    _ => query.OrderByDescending(p => p.CreatedAt) // "newest" default
                };

                var projects = await query
                    .Skip((searchDto.PageNumber - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .ToListAsync();

                var projectDtos = _mapper.Map<List<ProjectDto>>(projects);

                return new PagedResult<ProjectDto>
                {
                    Items = projectDtos,
                    PageNumber = searchDto.PageNumber,
                    PageSize = searchDto.PageSize,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in advanced project search");
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
                Attachments = _mapper.Map<List<FileAttachmentDto>>(await _unitOfWork.Repository<FileAttachment>()
                    .Query()
                    .Include(f => f.UploadedBy)
                    .Where(f => f.ProjectId == id && !f.IsDeleted)
                    .OrderByDescending(f => f.CreatedAt)
                    .ToListAsync()),
                IsOwner = project.ClientId == currentUserId,
                CanBid = !string.IsNullOrEmpty(currentUserId) && project.ClientId != currentUserId && !project.Bids.Any(b => b.FreelancerId == currentUserId),
                IsHiredFreelancer = project.Contract != null && project.Contract.FreelancerId == currentUserId
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
                project.CreatedAt = DateTime.UtcNow;
                project.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Projects.AddAsync(project);
                await _unitOfWork.SaveChangesAsync(); // Save first to get the ID

                // Add skills after project is saved
                if (dto.SkillIds != null && dto.SkillIds.Any())
                {
                    var existingSkillIds = await _unitOfWork.Repository<Skill>().Query()
                        .Where(s => dto.SkillIds.Contains(s.Id))
                        .Select(s => s.Id)
                        .ToListAsync();

                    foreach (var skillId in existingSkillIds)
                    {
                        var projectSkill = new ProjectSkill
                        {
                            ProjectId = project.Id,
                            SkillId = skillId
                        };
                        await _unitOfWork.Repository<ProjectSkill>().AddAsync(projectSkill);
                    }
                    await _unitOfWork.SaveChangesAsync(); // Save skills
                }

                var projectDto = await GetProjectByIdAsync(project.Id);
                return ApiResponse<ProjectDto>.SuccessResponse(projectDto!, Constants.Messages.ProjectCreated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project: {Message}", ex.Message);
                return ApiResponse<ProjectDto>.ErrorResponse($"Failed to create project: {ex.InnerException?.Message ?? ex.Message}");
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
            try
            {
                // 1. Fetch Freelancer Info
                var freelancerProfile = await _unitOfWork.FreelancerProfiles.Query()
                    .Include(fp => fp.FreelancerSkills)
                    .ThenInclude(fs => fs.Skill)
                    .FirstOrDefaultAsync(fp => fp.UserId == freelancerId);

                if (freelancerProfile == null)
                    return new List<ProjectDto>();

                var freelancerSkills = freelancerProfile.FreelancerSkills.Select(fs => fs.Skill.Name).ToList();
                var bio = freelancerProfile.Bio ?? "";

                // 2. Fetch Open Projects
                var openProjects = await _unitOfWork.Projects.Query()
                    .Include(p => p.Category)
                    .Include(p => p.Client)
                    .Include(p => p.ProjectSkills)
                        .ThenInclude(ps => ps.Skill)
                    .Where(p => p.Status == ProjectStatus.Open)
                    .ToListAsync();

                var projectInputs = openProjects.Select(p => new {
                    id = p.Id,
                    title = p.Title,
                    description = p.Description ?? "",
                    skills = p.ProjectSkills.Select(ps => ps.Skill.Name).ToList(),
                    category = p.Category?.Name ?? "Other",
                    budget = p.Budget
                }).ToList();

                var requestPayload = new {
                    freelancer_skills = freelancerSkills,
                    freelancer_bio = bio,
                    available_projects = projectInputs,
                    top_n = count
                };

                // 3. Call AI Microservice
                var aiUrl = _configuration["AiConfig:MicroserviceUrl"] ?? "http://localhost:8000";
                var aiEndpoint = $"{aiUrl}/recommend";
                
                var jsonPayload = JsonSerializer.Serialize(requestPayload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(aiEndpoint, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var aiResult = JsonDocument.Parse(responseJson);
                    var recommendations = aiResult.RootElement.GetProperty("recommendations");
                    
                    var recommendedProjectIds = new List<int>();
                    foreach (var rec in recommendations.EnumerateArray())
                    {
                        var pid = (int)rec.GetProperty("project_id").GetDouble();
                        recommendedProjectIds.Add(pid);
                    }
                    
                    // Order matching projects by the returned array
                    var orderedProjects = recommendedProjectIds
                        .Select(id => openProjects.FirstOrDefault(p => p.Id == id))
                        .Where(p => p != null)
                        .ToList();
                        
                    return _mapper.Map<List<ProjectDto>>(orderedProjects!);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to reach AI Microservice. Falling back to basic DB recommendations.");
            }

            // Fallback: Find projects matching skills via DB queries
            _logger.LogInformation("Using fallback DB query for recommendations.");
            
            var profile = await _unitOfWork.FreelancerProfiles.GetByUserIdAsync(freelancerId);
            var fsIds = profile?.FreelancerSkills.Select(fs => fs.SkillId).ToList() ?? new List<int>();

            var fallbackProjects = await _unitOfWork.Projects.Query()
                .Include(p => p.Category)
                .Include(p => p.Client)
                .Include(p => p.ProjectSkills)
                    .ThenInclude(ps => ps.Skill)
                .Where(p => p.Status == ProjectStatus.Open && 
                           p.ProjectSkills.Any(ps => fsIds.Contains(ps.SkillId)))
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();

            return _mapper.Map<List<ProjectDto>>(fallbackProjects);
        }
    }
}
