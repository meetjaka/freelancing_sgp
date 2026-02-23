using SGP_Freelancing.Models.Entities;

namespace SGP_Freelancing.Repositories.Interfaces
{
    public interface IProjectRepository : IRepository<Project>
    {
        Task<IEnumerable<Project>> GetOpenProjectsAsync();
        Task<IEnumerable<Project>> GetProjectsByCategoryAsync(int categoryId);
        Task<IEnumerable<Project>> GetProjectsByClientAsync(string clientId);
        Task<Project?> GetProjectWithBidsAsync(int projectId);
        Task<IEnumerable<Project>> SearchProjectsAsync(string searchTerm);
    }

    public interface IBidRepository : IRepository<Bid>
    {
        Task<IEnumerable<Bid>> GetBidsByProjectAsync(int projectId);
        Task<IEnumerable<Bid>> GetBidsByFreelancerAsync(string freelancerId);
        Task<Bid?> GetBidWithDetailsAsync(int bidId);
    }

    public interface IContractRepository : IRepository<Contract>
    {
        Task<IEnumerable<Contract>> GetActiveContractsAsync();
        Task<IEnumerable<Contract>> GetContractsByClientAsync(string clientId);
        Task<IEnumerable<Contract>> GetContractsByFreelancerAsync(string freelancerId);
        Task<Contract?> GetContractWithTransactionsAsync(int contractId);
    }

    public interface IFreelancerProfileRepository : IRepository<FreelancerProfile>
    {
        Task<FreelancerProfile?> GetByUserIdAsync(string userId);
        Task<FreelancerProfile?> GetWithSkillsAsync(int profileId);
        Task<IEnumerable<FreelancerProfile>> SearchFreelancersAsync(string searchTerm);
    }

    public interface IClientProfileRepository : IRepository<ClientProfile>
    {
        Task<ClientProfile?> GetByUserIdAsync(string userId);
        Task<ClientProfile?> GetWithProjectsAsync(int profileId);
    }

    public interface IMessageRepository : IRepository<Message>
    {
        Task<IEnumerable<Message>> GetUserMessagesAsync(string userId);
        Task<IEnumerable<Message>> GetConversationAsync(string user1Id, string user2Id);
        Task<int> GetUnreadCountAsync(string userId);
    }

    public interface IReviewRepository : IRepository<Review>
    {
        Task<IEnumerable<Review>> GetReviewsForUserAsync(string userId);
        Task<double> GetAverageRatingAsync(string userId);
    }

    public interface ICategoryRepository : IRepository<Category>
    {
        Task<Category?> GetCategoryWithProjectsAsync(int categoryId);
    }

    public interface ISkillRepository : IRepository<Skill>
    {
        Task<IEnumerable<Skill>> GetPopularSkillsAsync(int count);
    }

    public interface IPortfolioRepository : IRepository<Portfolio>
    {
        Task<Portfolio?> GetPortfolioWithDetailsAsync(int portfolioId);
        Task<Portfolio?> GetPortfolioByFreelancerAsync(int freelancerProfileId);
        Task<IEnumerable<Portfolio>> GetPublicPortfoliosAsync();
        Task<IEnumerable<Portfolio>> GetFeaturedPortfoliosAsync();
        Task<Portfolio?> GetPortfolioWithCasesAndTestimonialsAsync(int portfolioId);
    }

    public interface IPortfolioCaseRepository : IRepository<PortfolioCase>
    {
        Task<PortfolioCase?> GetCaseWithImagesAsync(int caseId);
        Task<IEnumerable<PortfolioCase>> GetCasesByPortfolioAsync(int portfolioId);
        Task<IEnumerable<PortfolioCase>> GetHighlightedCasesAsync(int portfolioId);
    }

    public interface IPortfolioImageRepository : IRepository<PortfolioImage>
    {
        Task<IEnumerable<PortfolioImage>> GetImagesByPortfolioCaseAsync(int caseId);
        Task<PortfolioImage?> GetThumbnailByCaseAsync(int caseId);
    }

    public interface IProjectTestimonialRepository : IRepository<ProjectTestimonial>
    {
        Task<IEnumerable<ProjectTestimonial>> GetTestimonialsByPortfolioAsync(int portfolioId);
        Task<IEnumerable<ProjectTestimonial>> GetApprovedTestimonialsAsync(int portfolioId);
        Task<decimal> GetAverageRatingAsync(int portfolioId);
    }
}
