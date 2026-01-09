using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGP_Freelancing.Models.Entities
{
    /// <summary>
    /// Project posted by clients
    /// </summary>
    public class Project : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;
        
        [Required]
        [Column(TypeName = "nvarchar(MAX)")]
        public string Description { get; set; } = null!;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Budget { get; set; }
        
        public ProjectStatus Status { get; set; } = ProjectStatus.Open;
        
        public DateTime? Deadline { get; set; }
        
        [Required]
        public string ClientId { get; set; } = null!;
        
        public int CategoryId { get; set; }
        
        // Navigation properties
        [ForeignKey(nameof(ClientId))]
        public ApplicationUser Client { get; set; } = null!;
        
        [ForeignKey(nameof(CategoryId))]
        public Category Category { get; set; } = null!;
        
        public ICollection<ProjectSkill> ProjectSkills { get; set; } = new List<ProjectSkill>();
        public ICollection<Bid> Bids { get; set; } = new List<Bid>();
        public Contract? Contract { get; set; }
    }
    
    public enum ProjectStatus
    {
        Open = 0,
        InProgress = 1,
        Completed = 2,
        Cancelled = 3,
        Closed = 4
    }
}
