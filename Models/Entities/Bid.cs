using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGP_Freelancing.Models.Entities
{
    /// <summary>
    /// Bid submitted by freelancers on projects
    /// </summary>
    public class Bid : BaseEntity
    {
        [Required]
        public string FreelancerId { get; set; } = null!;
        
        public int ProjectId { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal ProposedAmount { get; set; }
        
        public int EstimatedDurationDays { get; set; }
        
        [Required]
        [Column(TypeName = "nvarchar(MAX)")]
        public string CoverLetter { get; set; } = null!;
        
        public BidStatus Status { get; set; } = BidStatus.Pending;
        
        // Navigation properties
        [ForeignKey(nameof(FreelancerId))]
        public ApplicationUser Freelancer { get; set; } = null!;
        
        [ForeignKey(nameof(ProjectId))]
        public Project Project { get; set; } = null!;
    }
    
    public enum BidStatus
    {
        Pending = 0,
        Accepted = 1,
        Rejected = 2,
        Withdrawn = 3
    }
}
