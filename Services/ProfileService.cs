using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Models.Entities;
using SGP_Freelancing.Repositories;
using SGP_Freelancing.Services.Interfaces;
using SGP_Freelancing.Utilities;

namespace SGP_Freelancing.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ProfileService> _logger;

        public ProfileService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ProfileService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<FreelancerProfileDto>> GetFreelancerProfileAsync(string userId)
        {
            try
            {
                var profile = await _unitOfWork.FreelancerProfiles.GetByUserIdAsync(userId);

                if (profile == null)
                {
                    return ApiResponse<FreelancerProfileDto>.ErrorResponse("Freelancer profile not found");
                }

                var profileDto = _mapper.Map<FreelancerProfileDto>(profile);
                return ApiResponse<FreelancerProfileDto>.SuccessResponse(profileDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting freelancer profile");
                return ApiResponse<FreelancerProfileDto>.ErrorResponse("Failed to retrieve profile");
            }
        }

        public async Task<ApiResponse<FreelancerProfileDto>> UpdateFreelancerProfileAsync(string userId, UpdateFreelancerProfileDto dto)
        {
            try
            {
                var profile = await _unitOfWork.FreelancerProfiles.GetByUserIdAsync(userId);

                if (profile == null)
                {
                    // Create new profile if doesn't exist
                    profile = new FreelancerProfile
                    {
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.FreelancerProfiles.AddAsync(profile);
                    await _unitOfWork.SaveChangesAsync();
                }

                // Update profile fields
                profile.Title = dto.Title;
                profile.Bio = dto.Bio;
                profile.HourlyRate = dto.HourlyRate;
                profile.PortfolioUrl = dto.PortfolioUrl;
                profile.UpdatedAt = DateTime.UtcNow;

                var selectedSkillIds = dto.SkillIds?.Distinct().ToList() ?? new List<int>();
                var selectedSkills = selectedSkillIds.Count > 0
                    ? (await _unitOfWork.Repository<Skill>().FindAsync(s => selectedSkillIds.Contains(s.Id))).ToList()
                    : new List<Skill>();

                if (selectedSkillIds.Count > 0 && selectedSkills.Count != selectedSkillIds.Count)
                {
                    var missingSkillIds = selectedSkillIds.Except(selectedSkills.Select(s => s.Id)).ToList();
                    _logger.LogWarning("Some selected skills were not found for freelancer profile {UserId}: {SkillIds}", userId, string.Join(",", missingSkillIds));
                }

                var existingFreelancerSkills = profile.FreelancerSkills?.ToList() ?? new List<FreelancerSkill>();
                if (existingFreelancerSkills.Any())
                {
                    _unitOfWork.Repository<FreelancerSkill>().RemoveRange(existingFreelancerSkills);
                }

                profile.FreelancerSkills.Clear();
                foreach (var skill in selectedSkills)
                {
                    profile.FreelancerSkills.Add(new FreelancerSkill
                    {
                        FreelancerProfileId = profile.Id,
                        SkillId = skill.Id,
                        YearsOfExperience = 0
                    });
                }
                
                _unitOfWork.FreelancerProfiles.Update(profile);
                await _unitOfWork.SaveChangesAsync();

                var profileDto = _mapper.Map<FreelancerProfileDto>(profile);
                return ApiResponse<FreelancerProfileDto>.SuccessResponse(profileDto, "Profile updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating freelancer profile");
                return ApiResponse<FreelancerProfileDto>.ErrorResponse("Failed to update profile");
            }
        }

        public async Task<ApiResponse<ClientProfileDto>> GetClientProfileAsync(string userId)
        {
            try
            {
                var profile = await _unitOfWork.ClientProfiles
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (profile == null)
                {
                    return ApiResponse<ClientProfileDto>.ErrorResponse("Client profile not found");
                }

                var profileDto = _mapper.Map<ClientProfileDto>(profile);
                return ApiResponse<ClientProfileDto>.SuccessResponse(profileDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting client profile");
                return ApiResponse<ClientProfileDto>.ErrorResponse("Failed to retrieve profile");
            }
        }

        public async Task<ApiResponse<ClientProfileDto>> UpdateClientProfileAsync(string userId, UpdateClientProfileDto dto)
        {
            try
            {
                var profile = await _unitOfWork.ClientProfiles
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (profile == null)
                {
                    // Create new profile if doesn't exist
                    profile = new ClientProfile
                    {
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.ClientProfiles.AddAsync(profile);
                }

                // Update profile fields
                profile.CompanyName = dto.CompanyName;
                profile.CompanyDescription = dto.CompanyDescription;
                profile.Website = dto.Website;
                profile.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.ClientProfiles.Update(profile);
                await _unitOfWork.SaveChangesAsync();

                var profileDto = _mapper.Map<ClientProfileDto>(profile);
                return ApiResponse<ClientProfileDto>.SuccessResponse(profileDto, "Profile updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating client profile");
                return ApiResponse<ClientProfileDto>.ErrorResponse("Failed to update profile");
            }
        }
    }
}
