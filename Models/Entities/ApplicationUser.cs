using Microsoft.AspNetCore.Identity;

namespace SGP_Freelancing.Models.Entities
{
    /// <summary>
    /// Custom user entity extending IdentityUser
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public FreelancerProfile? FreelancerProfile { get; set; }
        public ClientProfile? ClientProfile { get; set; }
        public ICollection<Project> ClientProjects { get; set; } = new List<Project>();
        public ICollection<Bid> Bids { get; set; } = new List<Bid>();
        public ICollection<Contract> ClientContracts { get; set; } = new List<Contract>();
        public ICollection<Contract> FreelancerContracts { get; set; } = new List<Contract>();
        public ICollection<Review> ReviewsGiven { get; set; } = new List<Review>();
        public ICollection<Review> ReviewsReceived { get; set; } = new List<Review>();
        public ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
    }
}
