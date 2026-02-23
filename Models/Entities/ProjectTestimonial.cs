using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGP_Freelancing.Models.Entities
{
    /// <summary>
    /// Client testimonials for freelancer's portfolio
    /// </summary>
    public class ProjectTestimonial : BaseEntity
    {
        [Required]
        public int PortfolioId { get; set; }

        [Required]
        public string ClientName { get; set; } = null!;

        [StringLength(100)]
        public string? ClientCompany { get; set; }

        public string? ClientImageUrl { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(MAX)")]
        public string Content { get; set; } = null!;

        [Range(1, 5)]
        public decimal Rating { get; set; } = 5;

        public int? ProjectCaseId { get; set; }

        public DateTime? TestimonialDate { get; set; } = DateTime.UtcNow;

        public bool IsApproved { get; set; } = true;

        public bool IsAnonymous { get; set; } = false;

        public int DisplayOrder { get; set; } = 0;

        // Navigation properties
        [ForeignKey(nameof(PortfolioId))]
        public Portfolio Portfolio { get; set; } = null!;

        [ForeignKey(nameof(ProjectCaseId))]
        public PortfolioCase? ProjectCase { get; set; }
    }
}
