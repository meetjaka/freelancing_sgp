using System.ComponentModel.DataAnnotations;

namespace SGP_Freelancing.Models.DTOs
{
    // Project DTOs
    public class CreateProjectDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        [StringLength(5000)]
        public string Description { get; set; } = null!;

        [Required]
        [Range(0.01, 1000000)]
        public decimal Budget { get; set; }

        public DateTime? Deadline { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public List<int>? SkillIds { get; set; }
    }

    public class UpdateProjectDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        [StringLength(5000)]
        public string Description { get; set; } = null!;

        [Required]
        [Range(0.01, 1000000)]
        public decimal Budget { get; set; }

        public DateTime? Deadline { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public List<int>? SkillIds { get; set; }
    }

    public class ProjectDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Budget { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? Deadline { get; set; }
        public string ClientName { get; set; } = null!;
        public DateTime ClientCreatedAt { get; set; }
        public int ClientProjectsCount { get; set; }
        public string CategoryName { get; set; } = null!;
        public int BidsCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? ContractId { get; set; }
        public List<string> Skills { get; set; } = new();
    }

    // Bid DTOs
    public class CreateBidDto
    {
        [Required]
        public int ProjectId { get; set; }

        [Required]
        [Range(0.01, 1000000)]
        public decimal ProposedAmount { get; set; }

        [Required]
        [Range(1, 365)]
        public int EstimatedDurationDays { get; set; }

        [Required]
        [StringLength(2000)]
        public string CoverLetter { get; set; } = null!;
    }

    public class BidDto
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string ProjectTitle { get; set; } = null!;
        public decimal ProposedAmount { get; set; }
        public int EstimatedDurationDays { get; set; }
        public string CoverLetter { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string FreelancerName { get; set; } = null!;
        public decimal FreelancerRating { get; set; }
        public int FreelancerCompletedProjects { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Profile DTOs
    public class FreelancerProfileDto
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? FullName { get; set; }
        public string? Title { get; set; }
        public string? Bio { get; set; }
        public decimal HourlyRate { get; set; }
        public string? PortfolioUrl { get; set; }
        public int CompletedProjects { get; set; }
        public decimal AverageRating { get; set; }
        public List<SkillDto> Skills { get; set; } = new();
    }

    public class UpdateFreelancerProfileDto
    {
        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(2000)]
        public string? Bio { get; set; }

        [Range(0, 10000)]
        public decimal HourlyRate { get; set; }

        [Url]
        public string? PortfolioUrl { get; set; }

        public List<int>? SkillIds { get; set; }
    }

    public class ClientProfileDto
    {
        public int Id { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyDescription { get; set; }
        public string? Website { get; set; }
        public int TotalProjectsPosted { get; set; }
        public decimal AverageRating { get; set; }
    }

    public class UpdateClientProfileDto
    {
        [StringLength(200)]
        public string? CompanyName { get; set; }

        [StringLength(1000)]
        public string? CompanyDescription { get; set; }

        [Url]
        public string? Website { get; set; }
    }

    // Message DTOs
    public class SendMessageDto
    {
        [Required]
        public string ReceiverId { get; set; } = null!;

        [StringLength(200)]
        public string? Subject { get; set; }

        [Required]
        [StringLength(5000)]
        public string Content { get; set; } = null!;
    }

    public class MessageDto
    {
        public int Id { get; set; }
        public string SenderId { get; set; } = null!;
        public string SenderName { get; set; } = null!;
        public string ReceiverId { get; set; } = null!;
        public string ReceiverName { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string Content { get; set; } = null!;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<FileAttachmentDto> Attachments { get; set; } = new();
    }

    // Review DTOs
    public class CreateReviewDto
    {
        [Required]
        public int ContractId { get; set; }

        [Required]
        public string RevieweeId { get; set; } = null!;

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string? Comment { get; set; }
    }

    public class ReviewDto
    {
        public int Id { get; set; }
        public string ReviewerName { get; set; } = null!;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Contract DTOs
    public class CreateContractDto
    {
        [Required]
        public int ProjectId { get; set; }

        [Required]
        public int BidId { get; set; }

        [Required]
        public string FreelancerId { get; set; } = null!;

        [Required]
        [Range(0.01, 1000000)]
        public decimal Amount { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public string? Terms { get; set; }
    }

    public class ContractDto
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string ProjectTitle { get; set; } = null!;
        public string ClientId { get; set; } = null!;
        public string ClientName { get; set; } = null!;
        public string FreelancerId { get; set; } = null!;
        public string FreelancerName { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = null!;
        public string? Terms { get; set; }
    }

    // Skill & Category DTOs
    public class SkillDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? IconClass { get; set; }
    }

    // Portfolio DTOs
    public class CreatePortfolioDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = null!;

        [StringLength(1000)]
        public string? Description { get; set; }

        public string? DetailedBio { get; set; }

        public bool IsPublic { get; set; } = true;
    }

    public class UpdatePortfolioDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = null!;

        [StringLength(1000)]
        public string? Description { get; set; }

        public string? DetailedBio { get; set; }

        public bool IsPublic { get; set; }

        public bool IsFeatured { get; set; }
    }

    public class PortfolioDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? DetailedBio { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public bool IsPublic { get; set; }
        public bool IsFeatured { get; set; }
        public int ViewCount { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string FreelancerName { get; set; } = null!;
        public int TotalCases { get; set; }
        public int TotalTestimonials { get; set; }
        public decimal AverageRating { get; set; }
    }

    public class PortfolioDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? DetailedBio { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public bool IsPublic { get; set; }
        public int ViewCount { get; set; }
        public string FreelancerName { get; set; } = null!;
        public string? FreelancerImage { get; set; }
        public string? FreelancerTitle { get; set; }
        public decimal FreelancerRating { get; set; }
        public List<PortfolioCaseDto> Cases { get; set; } = new();
        public List<ProjectTestimonialDto> Testimonials { get; set; } = new();
    }

    // Portfolio Case DTOs
    public class CreatePortfolioCaseDto
    {
        [Required]
        public int PortfolioId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = null!;

        [StringLength(500)]
        public string? Description { get; set; }

        public string? DetailedDescription { get; set; }
        public string? ClientName { get; set; }
        public string? Industry { get; set; }
        public string? ProjectUrl { get; set; }
        public decimal? BudgetAmount { get; set; }
        public string? BudgetCurrency { get; set; } = "USD";
        public DateTime? CompletionDate { get; set; }
        public string? Technologies { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class UpdatePortfolioCaseDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = null!;

        [StringLength(500)]
        public string? Description { get; set; }

        public string? DetailedDescription { get; set; }
        public string? ClientName { get; set; }
        public string? Industry { get; set; }
        public string? ProjectUrl { get; set; }
        public decimal? BudgetAmount { get; set; }
        public string? BudgetCurrency { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string? Technologies { get; set; }
        public decimal? Rating { get; set; }
        public bool IsHighlighted { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class PortfolioCaseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? ClientName { get; set; }
        public string? Industry { get; set; }
        public string? ProjectUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public decimal? BudgetAmount { get; set; }
        public string? BudgetCurrency { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string? Technologies { get; set; }
        public decimal? Rating { get; set; }
        public bool IsHighlighted { get; set; }
        public int DisplayOrder { get; set; }
        public int ViewCount { get; set; }
        public List<PortfolioImageDto> Images { get; set; } = new();
    }

    // Portfolio Image DTOs
    public class CreatePortfolioImageDto
    {
        [Required]
        public int PortfolioCaseId { get; set; }

        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; } = null!;

        [StringLength(200)]
        public string? Caption { get; set; }

        public int DisplayOrder { get; set; }
        public bool IsThumbnail { get; set; }
    }

    public class PortfolioImageDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = null!;
        public string? Caption { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsThumbnail { get; set; }
        public int ViewCount { get; set; }
    }

    // Project Testimonial DTOs
    public class CreateProjectTestimonialDto
    {
        [Required]
        public int PortfolioId { get; set; }

        [Required]
        [StringLength(100)]
        public string ClientName { get; set; } = null!;

        [StringLength(100)]
        public string? ClientCompany { get; set; }

        [Required]
        public string Content { get; set; } = null!;

        [Range(1, 5)]
        public decimal Rating { get; set; } = 5;

        public int? ProjectCaseId { get; set; }
        public bool IsAnonymous { get; set; }
    }

    public class ProjectTestimonialDto
    {
        public int Id { get; set; }
        public string ClientName { get; set; } = null!;
        public string? ClientCompany { get; set; }
        public string? ClientImageUrl { get; set; }
        public string Content { get; set; } = null!;
        public decimal Rating { get; set; }
        public DateTime? TestimonialDate { get; set; }
        public bool IsAnonymous { get; set; }
        public int DisplayOrder { get; set; }
    }

    // Freelancer Search DTOs
    public class FreelancerSearchDto
    {
        public string? Search { get; set; }
        public List<int>? SkillIds { get; set; }
        public decimal? MinHourlyRate { get; set; }
        public decimal? MaxHourlyRate { get; set; }
        public decimal? MinRating { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public string? SortBy { get; set; } // "rating", "rate-asc", "rate-desc", "completed"
    }

    // Project Search DTO (Advanced Filters)
    public class ProjectSearchDto
    {
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public decimal? MinBudget { get; set; }
        public decimal? MaxBudget { get; set; }
        public List<int>? SkillIds { get; set; }
        public string? SortBy { get; set; } // "newest", "oldest", "budget-asc", "budget-desc", "most-bids", "deadline"
        public int? DeadlineWithinDays { get; set; } // e.g., projects due within 7, 14, 30 days
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }

    // Bookmark DTOs
    public class CreateBookmarkDto
    {
        [Required]
        public string BookmarkType { get; set; } = null!; // "Project" or "Freelancer"
        
        [Required]
        public int ItemId { get; set; }
        
        [StringLength(500)]
        public string? Note { get; set; }
    }

    public class BookmarkDto
    {
        public int Id { get; set; }
        public string BookmarkType { get; set; } = null!;
        public int ItemId { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Populated based on BookmarkType
        public string? ItemTitle { get; set; }
        public string? ItemDescription { get; set; }
        public decimal? ItemBudgetOrRate { get; set; }
        public string? ItemCategory { get; set; }
        public string? ItemOwnerName { get; set; }
        public decimal? ItemRating { get; set; }
        public int? ItemBidsCount { get; set; }
        public List<string>? ItemSkills { get; set; }
    }

    // ========== MILESTONE DTOs ==========
    public class MilestoneDto
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime? DueDate { get; set; }
        public int Order { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateMilestoneDto
    {
        [Required] public int ContractId { get; set; }
        [Required][StringLength(200)] public string Title { get; set; } = null!;
        [StringLength(2000)] public string? Description { get; set; }
        [Required][Range(0.01, 1000000)] public decimal Amount { get; set; }
        public DateTime? DueDate { get; set; }
        public int Order { get; set; }
    }

    // ========== GIG / SERVICE DTOs ==========
    public class FreelancerServiceDto
    {
        public int Id { get; set; }
        public string FreelancerId { get; set; } = null!;
        public string FreelancerName { get; set; } = null!;
        public string? FreelancerImage { get; set; }
        public decimal FreelancerRating { get; set; }
        public bool FreelancerIsVerified { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public decimal Price { get; set; }
        public int DeliveryDays { get; set; }
        public bool IsActive { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageRating { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Skills { get; set; } = new();
    }

    public class CreateFreelancerServiceDto
    {
        [Required][StringLength(200)] public string Title { get; set; } = null!;
        [Required][StringLength(5000)] public string Description { get; set; } = null!;
        [Required] public int CategoryId { get; set; }
        [Required][Range(1, 1000000)] public decimal Price { get; set; }
        [Required][Range(1, 365)] public int DeliveryDays { get; set; }
        public List<int>? SkillIds { get; set; }
    }

    public class ServiceOrderDto
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public string ServiceTitle { get; set; } = null!;
        public string ClientName { get; set; } = null!;
        public string FreelancerName { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Status { get; set; } = null!;
        public string? Requirements { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateServiceOrderDto
    {
        [Required] public int ServiceId { get; set; }
        [StringLength(5000)] public string? Requirements { get; set; }
    }

    // ========== CONNECTS DTOs ==========
    public class ConnectsWalletDto
    {
        public int Balance { get; set; }
        public int MonthlyAllocation { get; set; }
        public DateTime? LastRefillDate { get; set; }
        public List<ConnectsTransactionDto> RecentTransactions { get; set; } = new();
    }

    public class ConnectsTransactionDto
    {
        public int Id { get; set; }
        public int Amount { get; set; }
        public string Type { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ========== DISPUTE DTOs ==========
    public class DisputeDto
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public string ProjectTitle { get; set; } = null!;
        public string RaisedByName { get; set; } = null!;
        public string RaisedByUserId { get; set; } = null!;
        public string Reason { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? Resolution { get; set; }
        public string? ResolvedByAdminName { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ClientName { get; set; } = null!;
        public string FreelancerName { get; set; } = null!;
    }

    public class CreateDisputeDto
    {
        [Required] public int ContractId { get; set; }
        [Required][StringLength(5000)] public string Reason { get; set; } = null!;
    }

    public class ResolveDisputeDto
    {
        [Required] public int DisputeId { get; set; }
        [Required][StringLength(5000)] public string Resolution { get; set; } = null!;
    }

    // ========== KYC VERIFICATION DTOs ==========
    public class SubmitVerificationDto
    {
        [Required] public string DocumentUrl { get; set; } = null!;
    }

    public class VerificationStatusDto
    {
        public bool IsVerified { get; set; }
        public string Status { get; set; } = null!;
        public string? DocumentUrl { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? Note { get; set; }
    }

    public class AdminVerificationDto
    {
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? UserEmail { get; set; }
        public string? DocumentUrl { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? SubmittedAt { get; set; }
    }

    // ========== TIME TRACKING DTOs ==========
    public class TimeEntryDto
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public DateTime Date { get; set; }
        public decimal HoursWorked { get; set; }
        public string Description { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string FreelancerName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateTimeEntryDto
    {
        [Required] public int ContractId { get; set; }
        [Required] public DateTime Date { get; set; }
        [Required][Range(0.25, 24)] public decimal HoursWorked { get; set; }
        [Required][StringLength(500)] public string Description { get; set; } = null!;
    }

    public class TimesheetSummaryDto
    {
        public int ContractId { get; set; }
        public string ProjectTitle { get; set; } = null!;
        public decimal TotalHours { get; set; }
        public decimal ApprovedHours { get; set; }
        public decimal PendingHours { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal TotalEarnings { get; set; }
        public List<TimeEntryDto> Entries { get; set; } = new();
    }

    // Paging Result
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    // File Attachment DTOs
    public class FileAttachmentDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public string FileType { get; set; } = null!;
        public long FileSize { get; set; }
        public string UploadedById { get; set; } = null!;
        public string UploadedByName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public int? ProjectId { get; set; }
        public int? ContractId { get; set; }
        public int? MessageId { get; set; }
    }
}
