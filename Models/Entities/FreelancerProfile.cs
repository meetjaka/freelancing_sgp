using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGP_Freelancing.Models.Entities
{
    /// <summary>
    /// Freelancer profile with skills and portfolio
    /// </summary>
    public class FreelancerProfile : BaseEntity
    {
        [Required]
        public string UserId { get; set; } = null!;
        
        public string? Title { get; set; }
        
        [Column(TypeName = "nvarchar(MAX)")]
        public string? Bio { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal HourlyRate { get; set; }
        
        public string? PortfolioUrl { get; set; }
        public int TotalEarnings { get; set; }
        public int CompletedProjects { get; set; }
        public decimal AverageRating { get; set; }
        
        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;
        
        public ICollection<FreelancerSkill> FreelancerSkills { get; set; } = new List<FreelancerSkill>();
    }
}
