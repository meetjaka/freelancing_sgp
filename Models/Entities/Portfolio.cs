using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGP_Freelancing.Models.Entities
{
    /// <summary>
    /// Freelancer's portfolio information
    /// </summary>
    public class Portfolio : BaseEntity
    {
        [Required]
        public int FreelancerProfileId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = null!;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Column(TypeName = "nvarchar(MAX)")]
        public string? DetailedBio { get; set; }

        public string? ProfileImageUrl { get; set; }

        public string? CoverImageUrl { get; set; }

        public bool IsPublic { get; set; } = true;

        public bool IsFeatured { get; set; } = false;

        public int ViewCount { get; set; } = 0;

        public DateTime? PublishedAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(FreelancerProfileId))]
        public FreelancerProfile FreelancerProfile { get; set; } = null!;

        public ICollection<PortfolioCase> Cases { get; set; } = new List<PortfolioCase>();
        public ICollection<ProjectTestimonial> Testimonials { get; set; } = new List<ProjectTestimonial>();
    }
}
