using Microsoft.EntityFrameworkCore.Storage;
using SGP_Freelancing.Data;
using SGP_Freelancing.Repositories.Interfaces;

namespace SGP_Freelancing.Repositories
{
    /// <summary>
    /// Unit of Work implementation for transaction management
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;
        
        private IProjectRepository? _projects;
        private IBidRepository? _bids;
        private IContractRepository? _contracts;
        private IFreelancerProfileRepository? _freelancerProfiles;
        private IClientProfileRepository? _clientProfiles;
        private IMessageRepository? _messages;
        private IReviewRepository? _reviews;
        private ICategoryRepository? _categories;
        private ISkillRepository? _skills;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IProjectRepository Projects => _projects ??= new ProjectRepository(_context);
        public IBidRepository Bids => _bids ??= new BidRepository(_context);
        public IContractRepository Contracts => _contracts ??= new ContractRepository(_context);
        public IFreelancerProfileRepository FreelancerProfiles => _freelancerProfiles ??= new FreelancerProfileRepository(_context);
        public IClientProfileRepository ClientProfiles => _clientProfiles ??= new ClientProfileRepository(_context);
        public IMessageRepository Messages => _messages ??= new MessageRepository(_context);
        public IReviewRepository Reviews => _reviews ??= new ReviewRepository(_context);
        public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);
        public ISkillRepository Skills => _skills ??= new SkillRepository(_context);

        public IRepository<T> Repository<T>() where T : class
        {
            return new Repository<T>(_context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
