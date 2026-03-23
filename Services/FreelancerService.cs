using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Repositories;
using SGP_Freelancing.Services.Interfaces;
using SGP_Freelancing.Utilities;

namespace SGP_Freelancing.Services
{
    public class FreelancerService : IFreelancerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<FreelancerService> _logger;

        public FreelancerService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<FreelancerService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<PagedResult<FreelancerProfileDto>>> SearchFreelancersAsync(FreelancerSearchDto searchDto)
        {
            try
            {
                // Get all freelancers first, then filter in memory
                // TODO: Optimize with direct DB queries when IQueryable support is added
                var allProfiles = await _unitOfWork.FreelancerProfiles.GetAllAsync();
                var profiles = allProfiles.ToList();

                // Search by name, title, or bio
                if (!string.IsNullOrWhiteSpace(searchDto.Search))
                {
                    var searchLower = searchDto.Search.ToLower();
                    profiles = profiles.Where(p =>
                        (p.Title != null && p.Title.ToLower().Contains(searchLower)) ||
                        (p.Bio != null && p.Bio.ToLower().Contains(searchLower)) ||
                        (p.User != null && (p.User.FirstName + " " + p.User.LastName).ToLower().Contains(searchLower))
                    ).ToList();
                }

                // Filter by skills
                if (searchDto.SkillIds != null && searchDto.SkillIds.Any())
                {
                    profiles = profiles.Where(p => 
                        p.FreelancerSkills.Any(fs => searchDto.SkillIds.Contains(fs.SkillId))
                    ).ToList();
                }

                // Filter by hourly rate
                if (searchDto.MinHourlyRate.HasValue)
                {
                    profiles = profiles.Where(p => p.HourlyRate >= searchDto.MinHourlyRate.Value).ToList();
                }
                if (searchDto.MaxHourlyRate.HasValue)
                {
                    profiles = profiles.Where(p => p.HourlyRate <= searchDto.MaxHourlyRate.Value).ToList();
                }

                // Filter by rating
                if (searchDto.MinRating.HasValue)
                {
                    profiles = profiles.Where(p => p.AverageRating >= searchDto.MinRating.Value).ToList();
                }

                // Sorting
                profiles = searchDto.SortBy?.ToLower() switch
                {
                    "rating" => profiles.OrderByDescending(p => p.AverageRating).ToList(),
                    "rate-asc" => profiles.OrderBy(p => p.HourlyRate).ToList(),
                    "rate-desc" => profiles.OrderByDescending(p => p.HourlyRate).ToList(),
                    "completed" => profiles.OrderByDescending(p => p.CompletedProjects).ToList(),
                    "newest" => profiles.OrderByDescending(p => p.CreatedAt).ToList(),
                    _ => profiles.OrderByDescending(p => p.AverageRating).ThenByDescending(p => p.CompletedProjects).ToList()
                };

                // Get total count
                var totalCount = profiles.Count;

                // Apply pagination
                var items = profiles
                    .Skip((searchDto.Page - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .ToList();

                var itemDtos = _mapper.Map<List<FreelancerProfileDto>>(items);

                var result = new PagedResult<FreelancerProfileDto>
                {
                    Items = itemDtos,
                    TotalCount = totalCount,
                    PageNumber = searchDto.Page,
                    PageSize = searchDto.PageSize
                };

                return ApiResponse<PagedResult<FreelancerProfileDto>>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching freelancers");
                return ApiResponse<PagedResult<FreelancerProfileDto>>.ErrorResponse("Failed to search freelancers");
            }
        }

        public async Task<ApiResponse<FreelancerProfileDto>> GetFreelancerDetailAsync(int profileId)
        {
            try
            {
                var profile = await _unitOfWork.FreelancerProfiles.GetByIdAsync(profileId);

                if (profile == null)
                {
                    return ApiResponse<FreelancerProfileDto>.ErrorResponse("Freelancer not found");
                }

                var profileDto = _mapper.Map<FreelancerProfileDto>(profile);
                return ApiResponse<FreelancerProfileDto>.SuccessResponse(profileDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting freelancer detail");
                return ApiResponse<FreelancerProfileDto>.ErrorResponse("Failed to retrieve freelancer");
            }
        }

        public async Task<ApiResponse<List<FreelancerProfileDto>>> GetTopRatedFreelancersAsync(int count = 10)
        {
            try
            {
                var allFreelancers = await _unitOfWork.FreelancerProfiles.GetAllAsync();
                var freelancers = allFreelancers
                    .Where(p => p.AverageRating > 0)
                    .OrderByDescending(p => p.AverageRating)
                    .ThenByDescending(p => p.CompletedProjects)
                    .Take(count)
                    .ToList();

                var freelancerDtos = _mapper.Map<List<FreelancerProfileDto>>(freelancers);
                return ApiResponse<List<FreelancerProfileDto>>.SuccessResponse(freelancerDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top rated freelancers");
                return ApiResponse<List<FreelancerProfileDto>>.ErrorResponse("Failed to retrieve freelancers");
            }
        }
    }
}
