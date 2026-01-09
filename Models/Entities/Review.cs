using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGP_Freelancing.Models.Entities
{
    /// <summary>
    /// Review and rating between users
    /// </summary>
    public class Review : BaseEntity
    {
        public int ContractId { get; set; }
        
        [Required]
        public string ReviewerId { get; set; } = null!;
        
        [Required]
        public string RevieweeId { get; set; } = null!;
        
        [Range(1, 5)]
        public int Rating { get; set; }
        
        [Column(TypeName = "nvarchar(MAX)")]
        public string? Comment { get; set; }
        
        // Navigation properties
        [ForeignKey(nameof(ReviewerId))]
        public ApplicationUser Reviewer { get; set; } = null!;
        
        [ForeignKey(nameof(RevieweeId))]
        public ApplicationUser Reviewee { get; set; } = null!;
    }
}
