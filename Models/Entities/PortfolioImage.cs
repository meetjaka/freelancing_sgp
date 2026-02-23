using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGP_Freelancing.Models.Entities
{
    /// <summary>
    /// Images and media files for portfolio cases
    /// </summary>
    public class PortfolioImage : BaseEntity
    {
        [Required]
        public int PortfolioCaseId { get; set; }

        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; } = null!;

        [StringLength(200)]
        public string? Caption { get; set; }

        [StringLength(50)]
        public string? ImageType { get; set; } = "image"; // image, video, etc.

        public int DisplayOrder { get; set; } = 0;

        public long FileSizeBytes { get; set; } = 0;

        [StringLength(100)]
        public string? MimeType { get; set; }

        public bool IsThumbnail { get; set; } = false;

        public int ViewCount { get; set; } = 0;

        // Navigation properties
        [ForeignKey(nameof(PortfolioCaseId))]
        public PortfolioCase PortfolioCase { get; set; } = null!;
    }
}
