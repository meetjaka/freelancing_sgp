using Microsoft.EntityFrameworkCore;
using SGP_Freelancing.Data;
using SGP_Freelancing.Models.Entities;
using SGP_Freelancing.Repositories.Interfaces;

namespace SGP_Freelancing.Repositories
{
    public class ProjectRepository : Repository<Project>, IProjectRepository
    {
        public ProjectRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Project>> GetOpenProjectsAsync()
        {
            return await _dbSet
                .Where(p => p.Status == ProjectStatus.Open)
                .Include(p => p.Category)
                .Include(p => p.Client)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Project>> GetProjectsByCategoryAsync(int categoryId)
        {
            return await _dbSet
                .Where(p => p.CategoryId == categoryId)
                .Include(p => p.Category)
                .Include(p => p.Client)
                .ToListAsync();
        }

        public async Task<IEnumerable<Project>> GetProjectsByClientAsync(string clientId)
        {
            return await _dbSet
                .Where(p => p.ClientId == clientId)
                .Include(p => p.Category)
                .Include(p => p.Bids)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Project?> GetProjectWithBidsAsync(int projectId)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Client)
                .Include(p => p.Bids)
                    .ThenInclude(b => b.Freelancer)
                .FirstOrDefaultAsync(p => p.Id == projectId);
        }

        public async Task<IEnumerable<Project>> SearchProjectsAsync(string searchTerm)
        {
            return await _dbSet
                .Where(p => p.Title.Contains(searchTerm) || p.Description.Contains(searchTerm))
                .Include(p => p.Category)
                .Include(p => p.Client)
                .ToListAsync();
        }
    }

    public class BidRepository : Repository<Bid>, IBidRepository
    {
        public BidRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Bid>> GetBidsByProjectAsync(int projectId)
        {
            return await _dbSet
                .Where(b => b.ProjectId == projectId)
                .Include(b => b.Freelancer)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Bid>> GetBidsByFreelancerAsync(string freelancerId)
        {
            return await _dbSet
                .Where(b => b.FreelancerId == freelancerId)
                .Include(b => b.Project)
                    .ThenInclude(p => p.Category)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<Bid?> GetBidWithDetailsAsync(int bidId)
        {
            return await _dbSet
                .Include(b => b.Project)
                    .ThenInclude(p => p.Client)
                .Include(b => b.Freelancer)
                .FirstOrDefaultAsync(b => b.Id == bidId);
        }
    }

    public class ContractRepository : Repository<Contract>, IContractRepository
    {
        public ContractRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Contract>> GetActiveContractsAsync()
        {
            return await _dbSet
                .Where(c => c.Status == ContractStatus.Active)
                .Include(c => c.Project)
                .Include(c => c.Client)
                .Include(c => c.Freelancer)
                .ToListAsync();
        }

        public async Task<IEnumerable<Contract>> GetContractsByClientAsync(string clientId)
        {
            return await _dbSet
                .Where(c => c.ClientId == clientId)
                .Include(c => c.Project)
                .Include(c => c.Freelancer)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Contract>> GetContractsByFreelancerAsync(string freelancerId)
        {
            return await _dbSet
                .Where(c => c.FreelancerId == freelancerId)
                .Include(c => c.Project)
                .Include(c => c.Client)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Contract?> GetContractWithTransactionsAsync(int contractId)
        {
            return await _dbSet
                .Include(c => c.Project)
                .Include(c => c.Client)
                .Include(c => c.Freelancer)
                .Include(c => c.PaymentTransactions)
                .FirstOrDefaultAsync(c => c.Id == contractId);
        }
    }

    public class FreelancerProfileRepository : Repository<FreelancerProfile>, IFreelancerProfileRepository
    {
        public FreelancerProfileRepository(ApplicationDbContext context) : base(context) { }

        public async Task<FreelancerProfile?> GetByUserIdAsync(string userId)
        {
            return await _dbSet
                .Include(fp => fp.User)
                .Include(fp => fp.FreelancerSkills)
                    .ThenInclude(fs => fs.Skill)
                .FirstOrDefaultAsync(fp => fp.UserId == userId);
        }

        public async Task<FreelancerProfile?> GetWithSkillsAsync(int profileId)
        {
            return await _dbSet
                .Include(fp => fp.User)
                .Include(fp => fp.FreelancerSkills)
                    .ThenInclude(fs => fs.Skill)
                .FirstOrDefaultAsync(fp => fp.Id == profileId);
        }

        public async Task<IEnumerable<FreelancerProfile>> SearchFreelancersAsync(string searchTerm)
        {
            return await _dbSet
                .Where(fp => fp.Title != null && fp.Title.Contains(searchTerm) || 
                            fp.Bio != null && fp.Bio.Contains(searchTerm))
                .Include(fp => fp.User)
                .Include(fp => fp.FreelancerSkills)
                    .ThenInclude(fs => fs.Skill)
                .ToListAsync();
        }
    }

    public class ClientProfileRepository : Repository<ClientProfile>, IClientProfileRepository
    {
        public ClientProfileRepository(ApplicationDbContext context) : base(context) { }

        public async Task<ClientProfile?> GetByUserIdAsync(string userId)
        {
            return await _dbSet
                .Include(cp => cp.User)
                .FirstOrDefaultAsync(cp => cp.UserId == userId);
        }

        public async Task<ClientProfile?> GetWithProjectsAsync(int profileId)
        {
            return await _dbSet
                .Include(cp => cp.User)
                .Include(cp => cp.Projects)
                    .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(cp => cp.Id == profileId);
        }
    }

    public class MessageRepository : Repository<Message>, IMessageRepository
    {
        public MessageRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Message>> GetUserMessagesAsync(string userId)
        {
            return await _dbSet
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetConversationAsync(string user1Id, string user2Id)
        {
            return await _dbSet
                .Where(m => (m.SenderId == user1Id && m.ReceiverId == user2Id) ||
                           (m.SenderId == user2Id && m.ReceiverId == user1Id))
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _dbSet
                .CountAsync(m => m.ReceiverId == userId && !m.IsRead);
        }
    }

    public class ReviewRepository : Repository<Review>, IReviewRepository
    {
        public ReviewRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Review>> GetReviewsForUserAsync(string userId)
        {
            return await _dbSet
                .Where(r => r.RevieweeId == userId)
                .Include(r => r.Reviewer)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<double> GetAverageRatingAsync(string userId)
        {
            var reviews = await _dbSet
                .Where(r => r.RevieweeId == userId)
                .ToListAsync();

            return reviews.Any() ? reviews.Average(r => r.Rating) : 0;
        }
    }

    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Category?> GetCategoryWithProjectsAsync(int categoryId)
        {
            return await _dbSet
                .Include(c => c.Projects)
                .FirstOrDefaultAsync(c => c.Id == categoryId);
        }
    }

    public class SkillRepository : Repository<Skill>, ISkillRepository
    {
        public SkillRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Skill>> GetPopularSkillsAsync(int count)
        {
            return await _dbSet
                .OrderByDescending(s => s.FreelancerSkills.Count)
                .Take(count)
                .ToListAsync();
        }
    }

    public class PortfolioRepository : Repository<Portfolio>, IPortfolioRepository
    {
        public PortfolioRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Portfolio?> GetPortfolioWithDetailsAsync(int portfolioId)
        {
            return await _dbSet
                .Include(p => p.FreelancerProfile)
                    .ThenInclude(fp => fp.User)
                .Include(p => p.Cases)
                    .ThenInclude(pc => pc.Images)
                .Include(p => p.Testimonials)
                .FirstOrDefaultAsync(p => p.Id == portfolioId);
        }

        public async Task<Portfolio?> GetPortfolioByFreelancerAsync(int freelancerProfileId)
        {
            return await _dbSet
                .Include(p => p.FreelancerProfile)
                    .ThenInclude(fp => fp.User)
                .FirstOrDefaultAsync(p => p.FreelancerProfileId == freelancerProfileId);
        }

        public async Task<IEnumerable<Portfolio>> GetPublicPortfoliosAsync()
        {
            return await _dbSet
                .Where(p => p.IsPublic)
                .Include(p => p.FreelancerProfile)
                    .ThenInclude(fp => fp.User)
                .Include(p => p.Cases)
                .OrderByDescending(p => p.PublishedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Portfolio>> GetFeaturedPortfoliosAsync()
        {
            return await _dbSet
                .Where(p => p.IsFeatured && p.IsPublic)
                .Include(p => p.FreelancerProfile)
                    .ThenInclude(fp => fp.User)
                .Include(p => p.Cases)
                .OrderByDescending(p => p.PublishedAt)
                .ToListAsync();
        }

        public async Task<Portfolio?> GetPortfolioWithCasesAndTestimonialsAsync(int portfolioId)
        {
            return await _dbSet
                .Include(p => p.FreelancerProfile)
                    .ThenInclude(fp => fp.User)
                .Include(p => p.Cases)
                    .ThenInclude(pc => pc.Images)
                .Include(p => p.Testimonials)
                .FirstOrDefaultAsync(p => p.Id == portfolioId);
        }
    }

    public class PortfolioCaseRepository : Repository<PortfolioCase>, IPortfolioCaseRepository
    {
        public PortfolioCaseRepository(ApplicationDbContext context) : base(context) { }

        public async Task<PortfolioCase?> GetCaseWithImagesAsync(int caseId)
        {
            return await _dbSet
                .Include(pc => pc.Images)
                    .OrderBy(img => img.DisplayOrder)
                .FirstOrDefaultAsync(pc => pc.Id == caseId);
        }

        public async Task<IEnumerable<PortfolioCase>> GetCasesByPortfolioAsync(int portfolioId)
        {
            return await _dbSet
                .Where(pc => pc.PortfolioId == portfolioId)
                .Include(pc => pc.Images)
                    .OrderBy(img => img.DisplayOrder)
                .OrderBy(pc => pc.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<PortfolioCase>> GetHighlightedCasesAsync(int portfolioId)
        {
            return await _dbSet
                .Where(pc => pc.PortfolioId == portfolioId && pc.IsHighlighted)
                .Include(pc => pc.Images)
                    .OrderBy(img => img.DisplayOrder)
                .OrderBy(pc => pc.DisplayOrder)
                .ToListAsync();
        }
    }

    public class PortfolioImageRepository : Repository<PortfolioImage>, IPortfolioImageRepository
    {
        public PortfolioImageRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<PortfolioImage>> GetImagesByPortfolioCaseAsync(int caseId)
        {
            return await _dbSet
                .Where(pi => pi.PortfolioCaseId == caseId)
                .OrderBy(pi => pi.DisplayOrder)
                .ToListAsync();
        }

        public async Task<PortfolioImage?> GetThumbnailByCaseAsync(int caseId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(pi => pi.PortfolioCaseId == caseId && pi.IsThumbnail);
        }
    }

    public class ProjectTestimonialRepository : Repository<ProjectTestimonial>, IProjectTestimonialRepository
    {
        public ProjectTestimonialRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<ProjectTestimonial>> GetTestimonialsByPortfolioAsync(int portfolioId)
        {
            return await _dbSet
                .Where(pt => pt.PortfolioId == portfolioId)
                .OrderBy(pt => pt.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProjectTestimonial>> GetApprovedTestimonialsAsync(int portfolioId)
        {
            return await _dbSet
                .Where(pt => pt.PortfolioId == portfolioId && pt.IsApproved)
                .OrderBy(pt => pt.DisplayOrder)
                .ToListAsync();
        }

        public async Task<decimal> GetAverageRatingAsync(int portfolioId)
        {
            var testimonials = await _dbSet
                .Where(pt => pt.PortfolioId == portfolioId && pt.IsApproved)
                .ToListAsync();

            return testimonials.Any() ? testimonials.Average(t => t.Rating) : 0;
        }
    }
}
