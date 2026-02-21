using SGP_Freelancing.Models.DTOs;

namespace SGP_Freelancing.Models.ViewModels
{
    // Home ViewModels
    public class HomeViewModel
    {
        public List<CategoryDto> Categories { get; set; } = new();
        public List<ProjectDto> FeaturedProjects { get; set; } = new();
        public List<SkillDto> PopularSkills { get; set; } = new();
        public int TotalProjects { get; set; }
        public int TotalFreelancers { get; set; }
        public int TotalClients { get; set; }
    }

    // Project ViewModels
    public class ProjectListViewModel
    {
        public List<ProjectDto> Projects { get; set; } = new();
        public List<CategoryDto> Categories { get; set; } = new();
        public List<SkillDto> Skills { get; set; } = new();
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
    }

    public class ProjectDetailsViewModel
    {
        public ProjectDto Project { get; set; } = null!;
        public List<BidDto> Bids { get; set; } = new();
        public bool CanBid { get; set; }
        public bool IsOwner { get; set; }
    }

    public class CreateProjectViewModel
    {
        public CreateProjectDto Project { get; set; } = new();
        public List<CategoryDto> Categories { get; set; } = new();
        public List<SkillDto> Skills { get; set; } = new();
    }

    // Freelancer ViewModels
    public class FreelancerDashboardViewModel
    {
        public FreelancerProfileDto? Profile { get; set; }
        public List<BidDto> RecentBids { get; set; } = new();
        public List<ContractDto> ActiveContracts { get; set; } = new();
        public List<ProjectDto> RecommendedProjects { get; set; } = new();
        public int TotalEarnings { get; set; }
        public int UnreadMessages { get; set; }
    }

    public class FreelancerProfileViewModel
    {
        public FreelancerProfileDto Profile { get; set; } = null!;
        public List<ReviewDto> Reviews { get; set; } = new();
        public List<SkillDto> AllSkills { get; set; } = new();
    }

    // Client ViewModels
    public class ClientDashboardViewModel
    {
        public ClientProfileDto? Profile { get; set; }
        public List<ProjectDto> MyProjects { get; set; } = new();
        public List<ContractDto> ActiveContracts { get; set; } = new();
        public int TotalSpent { get; set; }
        public int UnreadMessages { get; set; }
    }

    public class ClientProfileViewModel
    {
        public ClientProfileDto Profile { get; set; } = null!;
        public List<ReviewDto> Reviews { get; set; } = new();
    }

    // Bid ViewModels
    public class CreateBidViewModel
    {
        public CreateBidDto Bid { get; set; } = new();
        public ProjectDto Project { get; set; } = null!;
    }

    // Message ViewModels
    public class MessagesViewModel
    {
        public List<MessageDto> Messages { get; set; } = new();
        public int UnreadCount { get; set; }
    }

    public class ConversationViewModel
    {
        public List<MessageDto> Messages { get; set; } = new();
        public string OtherUserName { get; set; } = null!;
        public string OtherUserId { get; set; } = null!;
    }

    // Contract ViewModels
    public class ContractDetailsViewModel
    {
        public ContractDto Contract { get; set; } = null!;
        public List<PaymentTransactionDto> Transactions { get; set; } = new();
        public bool CanReview { get; set; }
    }

    public class PaymentTransactionDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = null!;
        public string Type { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    // Admin ViewModels
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalProjects { get; set; }
        public int TotalContracts { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<ProjectDto> RecentProjects { get; set; } = new();
    }

    public class EarningsViewModel
    {
        public decimal TotalEarnings { get; set; }
        public decimal ThisMonthEarnings { get; set; }
        public decimal PercentageChange { get; set; }
        public decimal PendingEarnings { get; set; }
        public List<TransactionDto> RecentTransactions { get; set; } = new();
        public List<decimal> MonthlyData { get; set; } = new();
        public List<string> MonthlyLabels { get; set; } = new();
        public bool IsClient { get; set; }
    }

    public class TransactionDto
    {
        public DateTime Date { get; set; }
        public string ProjectTitle { get; set; } = null!;
        public string OtherPartyName { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Status { get; set; } = null!;
        public string Type { get; set; } = null!;
        public bool IsCredit { get; set; }
    }

    public class AnalyticsViewModel
    {
        public int ProfileViews { get; set; }
        public int DealsClosed { get; set; }
        public decimal SuccessRate { get; set; } // e.g. Accepted / Total
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public List<SkillProgressDto> TopSkills { get; set; } = new();
        public List<ActivityDto> RecentActivities { get; set; } = new();
        public bool IsClient { get; set; }
    }

    public class SkillProgressDto
    {
        public string SkillName { get; set; } = null!;
        public int Percentage { get; set; }
        public string ColorClass { get; set; } = null!;
    }

    public class ActivityDto
    {
        public string ActionName { get; set; } = null!;
        public string ProjectTitle { get; set; } = null!;
        public DateTime Date { get; set; }
        public string Status { get; set; } = null!;
        public string IconClass { get; set; } = null!;
        public string ColorClass { get; set; } = null!;
    }
}
