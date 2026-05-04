using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

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
        
        // KYC Verification
        public bool IsVerified { get; set; } = false;
        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.None;
        [MaxLength(500)]
        public string? VerificationDocumentUrl { get; set; }
        public DateTime? VerificationSubmittedAt { get; set; }
        public DateTime? VerificationReviewedAt { get; set; }
        [MaxLength(500)]
        public string? VerificationNote { get; set; }
        
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
        public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
        public ConnectsWallet? ConnectsWallet { get; set; }
    }

    public enum VerificationStatus
    {
        None = 0,
        Pending = 1,
        Approved = 2,
        Rejected = 3
    }
}
