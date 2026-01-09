using System.ComponentModel.DataAnnotations;

namespace SGP_Freelancing.Models.Entities
{
    /// <summary>
    /// Project category (e.g., Web Development, Mobile Apps, Design)
    /// </summary>
    public class Category : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public string? IconClass { get; set; }
        
        // Navigation properties
        public ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}
