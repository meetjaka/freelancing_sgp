using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SGP_Freelancing.Models;
using SGP_Freelancing.Models.Entities;

namespace SGP_Freelancing.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // OTP records (DB-backed, works on Render multi-instance)
        public DbSet<OtpRecord> OtpRecords { get; set; }

        // Old entities
        public DbSet<Student> Students { get; set; }
        
        // New entities
        public DbSet<FreelancerProfile> FreelancerProfiles { get; set; }
        public DbSet<ClientProfile> ClientProfiles { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<FreelancerSkill> FreelancerSkills { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectSkill> ProjectSkills { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure composite keys for many-to-many relationships
            modelBuilder.Entity<FreelancerSkill>()
                .HasKey(fs => new { fs.FreelancerProfileId, fs.SkillId });

            modelBuilder.Entity<ProjectSkill>()
                .HasKey(ps => new { ps.ProjectId, ps.SkillId });

            // Configure relationships
            ConfigureUserRelationships(modelBuilder);
            ConfigureProjectRelationships(modelBuilder);
            ConfigureContractRelationships(modelBuilder);
            ConfigureReviewRelationships(modelBuilder);
            ConfigureMessageRelationships(modelBuilder);

            // Configure indexes for performance
            ConfigureIndexes(modelBuilder);

            // Apply query filters for soft delete
            ApplyQueryFilters(modelBuilder);

            // Seed data
            SeedData(modelBuilder);
        }

        private void ConfigureUserRelationships(ModelBuilder modelBuilder)
        {
            // One-to-one: User -> FreelancerProfile
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.FreelancerProfile)
                .WithOne(fp => fp.User)
                .HasForeignKey<FreelancerProfile>(fp => fp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-one: User -> ClientProfile
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.ClientProfile)
                .WithOne(cp => cp.User)
                .HasForeignKey<ClientProfile>(cp => cp.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private void ConfigureProjectRelationships(ModelBuilder modelBuilder)
        {
            // Project -> Client
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Client)
                .WithMany(u => u.ClientProjects)
                .HasForeignKey(p => p.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Project -> Category
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Projects)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Project -> Bids
            modelBuilder.Entity<Project>()
                .HasMany(p => p.Bids)
                .WithOne(b => b.Project)
                .HasForeignKey(b => b.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Project -> Contract (One-to-One)
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Contract)
                .WithOne(c => c.Project)
                .HasForeignKey<Contract>(c => c.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private void ConfigureContractRelationships(ModelBuilder modelBuilder)
        {
            // Contract -> Client
            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Client)
                .WithMany(u => u.ClientContracts)
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Contract -> Freelancer
            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Freelancer)
                .WithMany(u => u.FreelancerContracts)
                .HasForeignKey(c => c.FreelancerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Contract -> PaymentTransactions
            modelBuilder.Entity<Contract>()
                .HasMany(c => c.PaymentTransactions)
                .WithOne(pt => pt.Contract)
                .HasForeignKey(pt => pt.ContractId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private void ConfigureReviewRelationships(ModelBuilder modelBuilder)
        {
            // Review -> Reviewer
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Reviewer)
                .WithMany(u => u.ReviewsGiven)
                .HasForeignKey(r => r.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Review -> Reviewee
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Reviewee)
                .WithMany(u => u.ReviewsReceived)
                .HasForeignKey(r => r.RevieweeId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private void ConfigureMessageRelationships(ModelBuilder modelBuilder)
        {
            // Message -> Sender
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Message -> Receiver
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private void ConfigureIndexes(ModelBuilder modelBuilder)
        {
            // Index on frequently queried fields
            modelBuilder.Entity<Project>()
                .HasIndex(p => p.Status);

            modelBuilder.Entity<Project>()
                .HasIndex(p => p.CreatedAt);

            modelBuilder.Entity<Bid>()
                .HasIndex(b => b.Status);

            modelBuilder.Entity<Message>()
                .HasIndex(m => m.IsRead);

            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.Email);
        }

        private void ApplyQueryFilters(ModelBuilder modelBuilder)
        {
            // Global query filter for soft delete
            modelBuilder.Entity<FreelancerProfile>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<ClientProfile>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Project>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Bid>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Contract>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Category>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Skill>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Review>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Message>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<PaymentTransaction>().HasQueryFilter(e => !e.IsDeleted);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Web Development", Description = "Websites and web applications", IconClass = "fa-globe", CreatedAt = DateTime.UtcNow },
                new Category { Id = 2, Name = "Mobile Development", Description = "iOS and Android apps", IconClass = "fa-mobile", CreatedAt = DateTime.UtcNow },
                new Category { Id = 3, Name = "Design & Creative", Description = "Graphic design, UI/UX", IconClass = "fa-palette", CreatedAt = DateTime.UtcNow },
                new Category { Id = 4, Name = "Writing & Translation", Description = "Content writing, copywriting", IconClass = "fa-pen", CreatedAt = DateTime.UtcNow },
                new Category { Id = 5, Name = "Data Science & AI", Description = "Machine learning, data analysis", IconClass = "fa-chart-line", CreatedAt = DateTime.UtcNow },
                new Category { Id = 6, Name = "Digital Marketing", Description = "SEO, social media marketing", IconClass = "fa-bullhorn", CreatedAt = DateTime.UtcNow }
            );

            // Seed Skills
            modelBuilder.Entity<Skill>().HasData(
                // Web Development
                new Skill { Id = 1, Name = "C#", Description = ".NET programming language", CreatedAt = DateTime.UtcNow },
                new Skill { Id = 2, Name = "ASP.NET Core", Description = "Web framework", CreatedAt = DateTime.UtcNow },
                new Skill { Id = 3, Name = "JavaScript", Description = "Frontend programming", CreatedAt = DateTime.UtcNow },
                new Skill { Id = 4, Name = "React", Description = "Frontend library", CreatedAt = DateTime.UtcNow },
                new Skill { Id = 5, Name = "Angular", Description = "Frontend framework", CreatedAt = DateTime.UtcNow },
                new Skill { Id = 6, Name = "Vue.js", Description = "Frontend framework", CreatedAt = DateTime.UtcNow },
                new Skill { Id = 7, Name = "Node.js", Description = "Backend JavaScript runtime", CreatedAt = DateTime.UtcNow },
                new Skill { Id = 8, Name = "Python", Description = "Programming language", CreatedAt = DateTime.UtcNow },
                new Skill { Id = 9, Name = "PHP", Description = "Server-side scripting", CreatedAt = DateTime.UtcNow },
                new Skill { Id = 10, Name = "SQL Server", Description = "Database management", CreatedAt = DateTime.UtcNow },
                // Mobile
                new Skill { Id = 11, Name = "Swift", Description = "iOS development", CreatedAt = DateTime.UtcNow },
                new Skill { Id = 12, Name = "Kotlin", Description = "Android development", CreatedAt = DateTime.UtcNow },
                new Skill { Id = 13, Name = "React Native", Description = "Cross-platform mobile", CreatedAt = DateTime.UtcNow },
                new Skill { Id = 14, Name = "Flutter", Description = "Cross-platform mobile", CreatedAt = DateTime.UtcNow },
                // Design
                new Skill { Id = 15, Name = "Adobe Photoshop", Description = "Image editing", CreatedAt = DateTime.UtcNow },
                new Skill { Id = 16, Name = "Figma", Description = "UI/UX design tool", CreatedAt = DateTime.UtcNow },
                new Skill { Id = 17, Name = "UI/UX Design", Description = "User interface design", CreatedAt = DateTime.UtcNow },
                // Other
                new Skill { Id = 18, Name = "Docker", Description = "Containerization", CreatedAt = DateTime.UtcNow },
                new Skill { Id = 19, Name = "Azure", Description = "Cloud platform", CreatedAt = DateTime.UtcNow },
                new Skill { Id = 20, Name = "Git", Description = "Version control", CreatedAt = DateTime.UtcNow }
            );
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }

        private void UpdateAuditFields()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (BaseEntity)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entity.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
    }
}
