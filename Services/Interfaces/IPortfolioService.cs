using AutoMapper;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Models.Entities;
using SGP_Freelancing.Repositories;

namespace SGP_Freelancing.Services.Interfaces
{
    public interface IPortfolioService
    {
        Task<PortfolioDto?> GetPortfolioByIdAsync(int portfolioId);
        Task<PortfolioDetailDto?> GetPortfolioDetailsAsync(int portfolioId);
        Task<PortfolioDto?> GetFreelancerPortfolioAsync(int freelancerProfileId);
        Task<IEnumerable<PortfolioDto>> GetPublicPortfoliosAsync();
        Task<IEnumerable<PortfolioDto>> GetFeaturedPortfoliosAsync();
        Task<PortfolioDto> CreatePortfolioAsync(string freelancerProfileId, CreatePortfolioDto createDto);
        Task<PortfolioDto> UpdatePortfolioAsync(int portfolioId, string userId, UpdatePortfolioDto updateDto);
        Task DeletePortfolioAsync(int portfolioId, string userId);
        Task<PortfolioDto> UpdatePortfolioImageAsync(int portfolioId, string userId, string imageUrl, string imageType);

        // Portfolio Case Methods
        Task<PortfolioCaseDto?> GetPortfolioCaseAsync(int caseId);
        Task<IEnumerable<PortfolioCaseDto>> GetPortfolioCasesAsync(int portfolioId);
        Task<PortfolioCaseDto> CreatePortfolioCaseAsync(string userId, CreatePortfolioCaseDto createDto);
        Task<PortfolioCaseDto> UpdatePortfolioCaseAsync(int caseId, string userId, UpdatePortfolioCaseDto updateDto);
        Task DeletePortfolioCaseAsync(int caseId, string userId);

        // Portfolio Image Methods
        Task<PortfolioImageDto> AddPortfolioImageAsync(string userId, CreatePortfolioImageDto createDto);
        Task DeletePortfolioImageAsync(int imageId, string userId);
        Task<IEnumerable<PortfolioImageDto>> GetPortfolioImagesAsync(int caseId);

        // Testimonial Methods
        Task<IEnumerable<ProjectTestimonialDto>> GetPortfolioTestimonialsAsync(int portfolioId);
        Task<ProjectTestimonialDto> AddTestimonialAsync(string userId, CreateProjectTestimonialDto createDto);
        Task DeleteTestimonialAsync(int testimonialId, string userId);
        Task<decimal> GetPortfolioAverageRatingAsync(int portfolioId);
    }
}

