using AutoMapper;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Models.Entities;
using SGP_Freelancing.Repositories;
using SGP_Freelancing.Services.Interfaces;

namespace SGP_Freelancing.Services
{
    public class PortfolioService : IPortfolioService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PortfolioService> _logger;

        public PortfolioService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PortfolioService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PortfolioDto?> GetPortfolioByIdAsync(int portfolioId)
        {
            try
            {
                var portfolio = await _unitOfWork.Portfolios.GetPortfolioWithDetailsAsync(portfolioId);
                if (portfolio == null) return null;

                var totalCases = portfolio.Cases.Count;
                var totalTestimonials = portfolio.Testimonials.Count;
                var avgRating = portfolio.Testimonials.Any() ? portfolio.Testimonials.Average(t => t.Rating) : 0;

                var portfolioDto = _mapper.Map<PortfolioDto>(portfolio);
                portfolioDto.TotalCases = totalCases;
                portfolioDto.TotalTestimonials = totalTestimonials;
                portfolioDto.AverageRating = (decimal)avgRating;

                return portfolioDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving portfolio {PortfolioId}", portfolioId);
                throw;
            }
        }

        public async Task<PortfolioDetailDto?> GetPortfolioDetailsAsync(int portfolioId)
        {
            try
            {
                var portfolio = await _unitOfWork.Portfolios.GetPortfolioWithCasesAndTestimonialsAsync(portfolioId);
                if (portfolio == null || !portfolio.IsPublic) return null;

                portfolio.ViewCount++;
                _unitOfWork.Portfolios.Update(portfolio);
                await _unitOfWork.SaveChangesAsync();

                var casesDto = _mapper.Map<List<PortfolioCaseDto>>(portfolio.Cases);
                var testimonialsDto = _mapper.Map<List<ProjectTestimonialDto>>(portfolio.Testimonials);

                var detailDto = new PortfolioDetailDto
                {
                    Id = portfolio.Id,
                    Title = portfolio.Title,
                    Description = portfolio.Description,
                    DetailedBio = portfolio.DetailedBio,
                    ProfileImageUrl = portfolio.ProfileImageUrl,
                    CoverImageUrl = portfolio.CoverImageUrl,
                    IsPublic = portfolio.IsPublic,
                    ViewCount = portfolio.ViewCount,
                    FreelancerName = portfolio.FreelancerProfile.User.UserName ?? "Anonymous",
                    FreelancerImage = portfolio.FreelancerProfile.User.ProfilePictureUrl,
                    FreelancerTitle = portfolio.FreelancerProfile.Title,
                    FreelancerRating = portfolio.FreelancerProfile.AverageRating,
                    Cases = casesDto,
                    Testimonials = testimonialsDto
                };

                return detailDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving portfolio details {PortfolioId}", portfolioId);
                throw;
            }
        }

        public async Task<PortfolioDto?> GetFreelancerPortfolioAsync(int freelancerProfileId)
        {
            try
            {
                var portfolio = await _unitOfWork.Portfolios.GetPortfolioByFreelancerAsync(freelancerProfileId);
                if (portfolio == null) return null;

                var totalCases = portfolio.Cases.Count;
                var totalTestimonials = portfolio.Testimonials.Count;
                var avgRating = portfolio.Testimonials.Any() ? portfolio.Testimonials.Average(t => t.Rating) : 0;

                var portfolioDto = _mapper.Map<PortfolioDto>(portfolio);
                portfolioDto.TotalCases = totalCases;
                portfolioDto.TotalTestimonials = totalTestimonials;
                portfolioDto.AverageRating = (decimal)avgRating;

                return portfolioDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving freelancer portfolio {FreelancerProfileId}", freelancerProfileId);
                throw;
            }
        }

        public async Task<IEnumerable<PortfolioDto>> GetPublicPortfoliosAsync()
        {
            try
            {
                var portfolios = await _unitOfWork.Portfolios.GetPublicPortfoliosAsync();
                var results = new List<PortfolioDto>();

                foreach (var portfolio in portfolios)
                {
                    var totalCases = portfolio.Cases.Count;
                    var totalTestimonials = portfolio.Testimonials.Count;
                    var avgRating = portfolio.Testimonials.Any() ? portfolio.Testimonials.Average(t => t.Rating) : 0;

                    var portfolioDto = _mapper.Map<PortfolioDto>(portfolio);
                    portfolioDto.TotalCases = totalCases;
                    portfolioDto.TotalTestimonials = totalTestimonials;
                    portfolioDto.AverageRating = (decimal)avgRating;

                    results.Add(portfolioDto);
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving public portfolios");
                throw;
            }
        }

        public async Task<IEnumerable<PortfolioDto>> GetFeaturedPortfoliosAsync()
        {
            try
            {
                var portfolios = await _unitOfWork.Portfolios.GetFeaturedPortfoliosAsync();
                var results = new List<PortfolioDto>();

                foreach (var portfolio in portfolios)
                {
                    var totalCases = portfolio.Cases.Count;
                    var totalTestimonials = portfolio.Testimonials.Count;
                    var avgRating = portfolio.Testimonials.Any() ? portfolio.Testimonials.Average(t => t.Rating) : 0;

                    var portfolioDto = _mapper.Map<PortfolioDto>(portfolio);
                    portfolioDto.TotalCases = totalCases;
                    portfolioDto.TotalTestimonials = totalTestimonials;
                    portfolioDto.AverageRating = (decimal)avgRating;

                    results.Add(portfolioDto);
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving featured portfolios");
                throw;
            }
        }

        public async Task<PortfolioDto> CreatePortfolioAsync(string userId, CreatePortfolioDto createDto)
        {
            try
            {
                var freelancerProfile = await _unitOfWork.FreelancerProfiles.GetByUserIdAsync(userId);
                if (freelancerProfile == null)
                    throw new InvalidOperationException("Freelancer profile not found");

                var portfolio = new Portfolio
                {
                    FreelancerProfileId = freelancerProfile.Id,
                    Title = createDto.Title,
                    Description = createDto.Description,
                    DetailedBio = createDto.DetailedBio,
                    IsPublic = createDto.IsPublic,
                    PublishedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Portfolios.AddAsync(portfolio);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Portfolio {PortfolioId} created for freelancer {FreelancerId}", portfolio.Id, freelancerProfile.Id);

                return _mapper.Map<PortfolioDto>(portfolio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating portfolio for freelancer {FreelancerId}", userId);
                throw;
            }
        }

        public async Task<PortfolioDto> UpdatePortfolioAsync(int portfolioId, string userId, UpdatePortfolioDto updateDto)
        {
            try
            {
                var portfolio = await _unitOfWork.Portfolios.GetByIdAsync(portfolioId);
                if (portfolio == null)
                    throw new InvalidOperationException("Portfolio not found");

                // Verify authorization (freelancer can only update their own portfolio)
                var freelancerProfile = await _unitOfWork.FreelancerProfiles.GetByUserIdAsync(userId);
                if (freelancerProfile?.Id != portfolio.FreelancerProfileId)
                    throw new UnauthorizedAccessException("You can only update your own portfolio");

                portfolio.Title = updateDto.Title;
                portfolio.Description = updateDto.Description;
                portfolio.DetailedBio = updateDto.DetailedBio;
                portfolio.IsPublic = updateDto.IsPublic;
                portfolio.IsFeatured = updateDto.IsFeatured;
                portfolio.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Portfolios.Update(portfolio);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Portfolio {PortfolioId} updated", portfolioId);

                return _mapper.Map<PortfolioDto>(portfolio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating portfolio {PortfolioId}", portfolioId);
                throw;
            }
        }

        public async Task DeletePortfolioAsync(int portfolioId, string userId)
        {
            try
            {
                var portfolio = await _unitOfWork.Portfolios.GetByIdAsync(portfolioId);
                if (portfolio == null)
                    throw new InvalidOperationException("Portfolio not found");

                // Verify authorization
                var freelancerProfile = await _unitOfWork.FreelancerProfiles.GetByUserIdAsync(userId);
                if (freelancerProfile?.Id != portfolio.FreelancerProfileId)
                    throw new UnauthorizedAccessException("You can only delete your own portfolio");

                portfolio.IsDeleted = true;
                portfolio.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Portfolios.Update(portfolio);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Portfolio {PortfolioId} deleted", portfolioId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting portfolio {PortfolioId}", portfolioId);
                throw;
            }
        }

        public async Task<PortfolioDto> UpdatePortfolioImageAsync(int portfolioId, string userId, string imageUrl, string imageType)
        {
            try
            {
                var portfolio = await _unitOfWork.Portfolios.GetByIdAsync(portfolioId);
                if (portfolio == null)
                    throw new InvalidOperationException("Portfolio not found");

                // Verify authorization
                var freelancerProfile = await _unitOfWork.FreelancerProfiles.GetByUserIdAsync(userId);
                if (freelancerProfile?.Id != portfolio.FreelancerProfileId)
                    throw new UnauthorizedAccessException("You can only update your own portfolio");

                if (imageType == "profile")
                    portfolio.ProfileImageUrl = imageUrl;
                else if (imageType == "cover")
                    portfolio.CoverImageUrl = imageUrl;

                portfolio.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Portfolios.Update(portfolio);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Portfolio {PortfolioId} image updated", portfolioId);

                return _mapper.Map<PortfolioDto>(portfolio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating portfolio image {PortfolioId}", portfolioId);
                throw;
            }
        }

        // Portfolio Case Methods

        public async Task<PortfolioCaseDto?> GetPortfolioCaseAsync(int caseId)
        {
            try
            {
                var portfolioCase = await _unitOfWork.PortfolioCases.GetCaseWithImagesAsync(caseId);
                if (portfolioCase == null) return null;

                portfolioCase.ViewCount++;
                _unitOfWork.PortfolioCases.Update(portfolioCase);
                await _unitOfWork.SaveChangesAsync();

                return _mapper.Map<PortfolioCaseDto>(portfolioCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving portfolio case {CaseId}", caseId);
                throw;
            }
        }

        public async Task<IEnumerable<PortfolioCaseDto>> GetPortfolioCasesAsync(int portfolioId)
        {
            try
            {
                var cases = await _unitOfWork.PortfolioCases.GetCasesByPortfolioAsync(portfolioId);
                return _mapper.Map<List<PortfolioCaseDto>>(cases);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving portfolio cases {PortfolioId}", portfolioId);
                throw;
            }
        }

        public async Task<PortfolioCaseDto> CreatePortfolioCaseAsync(string userId, CreatePortfolioCaseDto createDto)
        {
            try
            {
                // Verify authorization
                var freelancerProfile = await _unitOfWork.FreelancerProfiles.GetByUserIdAsync(userId);
                var portfolio = await _unitOfWork.Portfolios.GetByIdAsync(createDto.PortfolioId);
                
                if (portfolio == null)
                    throw new InvalidOperationException("Portfolio not found");
                if (freelancerProfile?.Id != portfolio.FreelancerProfileId)
                    throw new UnauthorizedAccessException("You can only add cases to your own portfolio");

                var portfolioCase = new PortfolioCase
                {
                    PortfolioId = createDto.PortfolioId,
                    Title = createDto.Title,
                    Description = createDto.Description,
                    DetailedDescription = createDto.DetailedDescription,
                    ClientName = createDto.ClientName,
                    Industry = createDto.Industry,
                    ProjectUrl = createDto.ProjectUrl,
                    BudgetAmount = createDto.BudgetAmount,
                    BudgetCurrency = createDto.BudgetCurrency ?? "USD",
                    CompletionDate = createDto.CompletionDate,
                    Technologies = createDto.Technologies,
                    DisplayOrder = createDto.DisplayOrder,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.PortfolioCases.AddAsync(portfolioCase);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Portfolio case {CaseId} created for portfolio {PortfolioId}", portfolioCase.Id, createDto.PortfolioId);

                return _mapper.Map<PortfolioCaseDto>(portfolioCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating portfolio case");
                throw;
            }
        }

        public async Task<PortfolioCaseDto> UpdatePortfolioCaseAsync(int caseId, string userId, UpdatePortfolioCaseDto updateDto)
        {
            try
            {
                var portfolioCase = await _unitOfWork.PortfolioCases.GetByIdAsync(caseId);
                if (portfolioCase == null)
                    throw new InvalidOperationException("Portfolio case not found");

                var portfolio = await _unitOfWork.Portfolios.GetByIdAsync(portfolioCase.PortfolioId);
                var freelancerProfile = await _unitOfWork.FreelancerProfiles.GetByUserIdAsync(userId);
                
                if (freelancerProfile?.Id != portfolio?.FreelancerProfileId)
                    throw new UnauthorizedAccessException("You can only update your own portfolio cases");

                portfolioCase.Title = updateDto.Title;
                portfolioCase.Description = updateDto.Description;
                portfolioCase.DetailedDescription = updateDto.DetailedDescription;
                portfolioCase.ClientName = updateDto.ClientName;
                portfolioCase.Industry = updateDto.Industry;
                portfolioCase.ProjectUrl = updateDto.ProjectUrl;
                portfolioCase.BudgetAmount = updateDto.BudgetAmount;
                portfolioCase.BudgetCurrency = updateDto.BudgetCurrency;
                portfolioCase.CompletionDate = updateDto.CompletionDate;
                portfolioCase.Technologies = updateDto.Technologies;
                portfolioCase.Rating = updateDto.Rating;
                portfolioCase.IsHighlighted = updateDto.IsHighlighted;
                portfolioCase.DisplayOrder = updateDto.DisplayOrder;
                portfolioCase.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.PortfolioCases.Update(portfolioCase);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Portfolio case {CaseId} updated", caseId);

                return _mapper.Map<PortfolioCaseDto>(portfolioCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating portfolio case {CaseId}", caseId);
                throw;
            }
        }

        public async Task DeletePortfolioCaseAsync(int caseId, string userId)
        {
            try
            {
                var portfolioCase = await _unitOfWork.PortfolioCases.GetByIdAsync(caseId);
                if (portfolioCase == null)
                    throw new InvalidOperationException("Portfolio case not found");

                var portfolio = await _unitOfWork.Portfolios.GetByIdAsync(portfolioCase.PortfolioId);
                var freelancerProfile = await _unitOfWork.FreelancerProfiles.GetByUserIdAsync(userId);
                
                if (freelancerProfile?.Id != portfolio?.FreelancerProfileId)
                    throw new UnauthorizedAccessException("You can only delete your own portfolio cases");

                portfolioCase.IsDeleted = true;
                portfolioCase.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.PortfolioCases.Update(portfolioCase);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Portfolio case {CaseId} deleted", caseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting portfolio case {CaseId}", caseId);
                throw;
            }
        }

        // Portfolio Image Methods

        public async Task<PortfolioImageDto> AddPortfolioImageAsync(string userId, CreatePortfolioImageDto createDto)
        {
            try
            {
                var portfolioCase = await _unitOfWork.PortfolioCases.GetByIdAsync(createDto.PortfolioCaseId);
                if (portfolioCase == null)
                    throw new InvalidOperationException("Portfolio case not found");

                var portfolio = await _unitOfWork.Portfolios.GetByIdAsync(portfolioCase.PortfolioId);
                var freelancerProfile = await _unitOfWork.FreelancerProfiles.GetByUserIdAsync(userId);
                
                if (freelancerProfile?.Id != portfolio?.FreelancerProfileId)
                    throw new UnauthorizedAccessException("You can only add images to your own portfolio");

                // If this is the first image and marked as thumbnail, set it
                var existingImages = await _unitOfWork.PortfolioImages.GetImagesByPortfolioCaseAsync(createDto.PortfolioCaseId);
                var isThumbnail = createDto.IsThumbnail || !existingImages.Any();

                var portfolioImage = new PortfolioImage
                {
                    PortfolioCaseId = createDto.PortfolioCaseId,
                    ImageUrl = createDto.ImageUrl,
                    Caption = createDto.Caption,
                    DisplayOrder = createDto.DisplayOrder,
                    IsThumbnail = isThumbnail,
                    CreatedAt = DateTime.UtcNow
                };

                // If this is thumbnail, unset other thumbnails
                if (isThumbnail)
                {
                    foreach (var oldImage in existingImages.Where(x => x.IsThumbnail))
                    {
                        oldImage.IsThumbnail = false;
                        _unitOfWork.PortfolioImages.Update(oldImage);
                    }
                }

                // Update portfolio case thumbnail
                portfolioCase.ThumbnailUrl = portfolioImage.ImageUrl;
                _unitOfWork.PortfolioCases.Update(portfolioCase);

                await _unitOfWork.PortfolioImages.AddAsync(portfolioImage);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Portfolio image {ImageId} added to case {CaseId}", portfolioImage.Id, createDto.PortfolioCaseId);

                return _mapper.Map<PortfolioImageDto>(portfolioImage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding portfolio image");
                throw;
            }
        }

        public async Task DeletePortfolioImageAsync(int imageId, string userId)
        {
            try
            {
                var portfolioImage = await _unitOfWork.PortfolioImages.GetByIdAsync(imageId);
                if (portfolioImage == null)
                    throw new InvalidOperationException("Portfolio image not found");

                var portfolioCase = await _unitOfWork.PortfolioCases.GetByIdAsync(portfolioImage.PortfolioCaseId);
                var portfolio = await _unitOfWork.Portfolios.GetByIdAsync(portfolioCase?.PortfolioId ?? 0);
                var freelancerProfile = await _unitOfWork.FreelancerProfiles.GetByUserIdAsync(userId);
                
                if (freelancerProfile?.Id != portfolio?.FreelancerProfileId)
                    throw new UnauthorizedAccessException("You can only delete your own portfolio images");

                portfolioImage.IsDeleted = true;
                portfolioImage.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.PortfolioImages.Update(portfolioImage);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Portfolio image {ImageId} deleted", imageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting portfolio image {ImageId}", imageId);
                throw;
            }
        }

        public async Task<IEnumerable<PortfolioImageDto>> GetPortfolioImagesAsync(int caseId)
        {
            try
            {
                var images = await _unitOfWork.PortfolioImages.GetImagesByPortfolioCaseAsync(caseId);
                return _mapper.Map<List<PortfolioImageDto>>(images);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving portfolio images for case {CaseId}", caseId);
                throw;
            }
        }

        // Testimonial Methods

        public async Task<IEnumerable<ProjectTestimonialDto>> GetPortfolioTestimonialsAsync(int portfolioId)
        {
            try
            {
                var testimonials = await _unitOfWork.ProjectTestimonials.GetApprovedTestimonialsAsync(portfolioId);
                return _mapper.Map<List<ProjectTestimonialDto>>(testimonials);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving portfolio testimonials {PortfolioId}", portfolioId);
                throw;
            }
        }

        public async Task<ProjectTestimonialDto> AddTestimonialAsync(string userId, CreateProjectTestimonialDto createDto)
        {
            try
            {
                var portfolio = await _unitOfWork.Portfolios.GetByIdAsync(createDto.PortfolioId);
                if (portfolio == null)
                    throw new InvalidOperationException("Portfolio not found");

                var testimonial = new ProjectTestimonial
                {
                    PortfolioId = createDto.PortfolioId,
                    ClientName = createDto.IsAnonymous ? "Anonymous" : createDto.ClientName,
                    ClientCompany = createDto.ClientCompany,
                    Content = createDto.Content,
                    Rating = createDto.Rating,
                    ProjectCaseId = createDto.ProjectCaseId,
                    IsAnonymous = createDto.IsAnonymous,
                    TestimonialDate = DateTime.UtcNow,
                    IsApproved = true, // Auto-approve for now
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.ProjectTestimonials.AddAsync(testimonial);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Testimonial {TestimonialId} added to portfolio {PortfolioId}", testimonial.Id, createDto.PortfolioId);

                return _mapper.Map<ProjectTestimonialDto>(testimonial);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding testimonial");
                throw;
            }
        }

        public async Task DeleteTestimonialAsync(int testimonialId, string userId)
        {
            try
            {
                var testimonial = await _unitOfWork.ProjectTestimonials.GetByIdAsync(testimonialId);
                if (testimonial == null)
                    throw new InvalidOperationException("Testimonial not found");

                var portfolio = await _unitOfWork.Portfolios.GetByIdAsync(testimonial.PortfolioId);
                var freelancerProfile = await _unitOfWork.FreelancerProfiles.GetByUserIdAsync(userId);
                
                if (freelancerProfile?.Id != portfolio?.FreelancerProfileId)
                    throw new UnauthorizedAccessException("You can only delete testimonials on your own portfolio");

                testimonial.IsDeleted = true;
                testimonial.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.ProjectTestimonials.Update(testimonial);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Testimonial {TestimonialId} deleted", testimonialId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting testimonial {TestimonialId}", testimonialId);
                throw;
            }
        }

        public async Task<decimal> GetPortfolioAverageRatingAsync(int portfolioId)
        {
            try
            {
                return await _unitOfWork.ProjectTestimonials.GetAverageRatingAsync(portfolioId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating portfolio average rating {PortfolioId}", portfolioId);
                throw;
            }
        }
    }
}

