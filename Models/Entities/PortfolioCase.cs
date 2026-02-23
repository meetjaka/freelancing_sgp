using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGP_Freelancing.Models.Entities
{
    /// <summary>
    /// Individual case study/work sample in a portfolio
    /// </summary>
    public class PortfolioCase : BaseEntity
    {
        [Required]
        public int PortfolioId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = null!;

        [StringLength(500)]
        public string? Description { get; set; }

        [Column(TypeName = "nvarchar(MAX)")]
        public string? DetailedDescription { get; set; }

        public string? ClientName { get; set; }

        public string? Industry { get; set; }

        [StringLength(500)]
        public string? ProjectUrl { get; set; }

        public string? ThumbnailUrl { get; set; }

        public decimal? BudgetAmount { get; set; }

        [StringLength(50)]
        public string? BudgetCurrency { get; set; } = "USD";

        public DateTime? CompletionDate { get; set; }

        [StringLength(500)]
        public string? Technologies { get; set; }

        public decimal? Rating { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsHighlighted { get; set; } = false;

        public int ViewCount { get; set; } = 0;

        // Navigation properties
        [ForeignKey(nameof(PortfolioId))]
        public Portfolio Portfolio { get; set; } = null!;

        public ICollection<PortfolioImage> Images { get; set; } = new List<PortfolioImage>();
    }
}
