using SGP_Freelancing.Repositories.Interfaces;

namespace SGP_Freelancing.Repositories
{
    /// <summary>
    /// Unit of Work pattern to manage transactions across multiple repositories
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IProjectRepository Projects { get; }
        IBidRepository Bids { get; }
        IContractRepository Contracts { get; }
        IFreelancerProfileRepository FreelancerProfiles { get; }
        IClientProfileRepository ClientProfiles { get; }
        IMessageRepository Messages { get; }
        IReviewRepository Reviews { get; }
        ICategoryRepository Categories { get; }
        ISkillRepository Skills { get; }
        IPortfolioRepository Portfolios { get; }
        IPortfolioCaseRepository PortfolioCases { get; }
        IPortfolioImageRepository PortfolioImages { get; }
        IProjectTestimonialRepository ProjectTestimonials { get; }
        IRepository<T> Repository<T>() where T : class;
        
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
