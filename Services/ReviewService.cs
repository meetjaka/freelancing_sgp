using Microsoft.EntityFrameworkCore;
using SGP_Freelancing.Data;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Models.Entities;
using SGP_Freelancing.Services.Interfaces;
using SGP_Freelancing.Utilities;

namespace SGP_Freelancing.Services
{
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext _context;

        public ReviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<ReviewDto>> CreateReviewAsync(CreateReviewDto dto, string reviewerId)
        {
            try
            {
                // Verify contract exists and is completed
                var contract = await _context.Contracts
                    .Include(c => c.Project)
                    .FirstOrDefaultAsync(c => c.Id == dto.ContractId && !c.IsDeleted);

                if (contract == null)
                    return ApiResponse<ReviewDto>.ErrorResponse("Contract not found.");

                if (contract.Status != ContractStatus.Completed)
                    return ApiResponse<ReviewDto>.ErrorResponse("You can only review completed contracts.");

                // Verify the reviewer was part of the contract
                if (contract.ClientId != reviewerId && contract.FreelancerId != reviewerId)
                    return ApiResponse<ReviewDto>.ErrorResponse("You are not part of this contract.");

                // Prevent duplicate reviews
                var existingReview = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.ContractId == dto.ContractId && r.ReviewerId == reviewerId && !r.IsDeleted);

                if (existingReview != null)
                    return ApiResponse<ReviewDto>.ErrorResponse("You have already reviewed this contract.");

                var reviewer = await _context.Users.FindAsync(reviewerId);
                if (reviewer == null)
                    return ApiResponse<ReviewDto>.ErrorResponse("Reviewer not found.");

                var review = new Review
                {
                    ContractId = dto.ContractId,
                    ReviewerId = reviewerId,
                    RevieweeId = dto.RevieweeId,
                    Rating = dto.Rating,
                    Comment = dto.Comment
                };

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                var reviewDto = new ReviewDto
                {
                    Id = review.Id,
                    ReviewerName = $"{reviewer.FirstName} {reviewer.LastName}",
                    Rating = review.Rating,
                    Comment = review.Comment,
                    CreatedAt = review.CreatedAt
                };

                return ApiResponse<ReviewDto>.SuccessResponse(reviewDto, "Review submitted successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<ReviewDto>.ErrorResponse("An error occurred while submitting the review.");
            }
        }

        public async Task<List<ReviewDto>> GetReviewsByUserAsync(string userId)
        {
            var reviews = await _context.Reviews
                .Include(r => r.Reviewer)
                .Where(r => r.RevieweeId == userId && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    ReviewerName = $"{r.Reviewer.FirstName} {r.Reviewer.LastName}",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return reviews;
        }

        public async Task<List<ReviewDto>> GetReviewsByContractAsync(int contractId)
        {
            var reviews = await _context.Reviews
                .Include(r => r.Reviewer)
                .Where(r => r.ContractId == contractId && !r.IsDeleted)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    ReviewerName = $"{r.Reviewer.FirstName} {r.Reviewer.LastName}",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return reviews;
        }

        public async Task<ReviewDto?> GetReviewAsync(int contractId, string reviewerId)
        {
            var review = await _context.Reviews
                .Include(r => r.Reviewer)
                .FirstOrDefaultAsync(r => r.ContractId == contractId && r.ReviewerId == reviewerId && !r.IsDeleted);

            if (review == null) return null;

            return new ReviewDto
            {
                Id = review.Id,
                ReviewerName = $"{review.Reviewer.FirstName} {review.Reviewer.LastName}",
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            };
        }
    }
}
