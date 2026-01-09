using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGP_Freelancing.Models.Entities
{
    /// <summary>
    /// Client profile for posting projects
    /// </summary>
    public class ClientProfile : BaseEntity
    {
        [Required]
        public string UserId { get; set; } = null!;
        
        public string? CompanyName { get; set; }
        
        [Column(TypeName = "nvarchar(MAX)")]
        public string? CompanyDescription { get; set; }
        
        public string? Website { get; set; }
        public int TotalProjectsPosted { get; set; }
        public decimal AverageRating { get; set; }
        
        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;
        
        public ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}
