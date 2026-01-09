using System.ComponentModel.DataAnnotations;

namespace SGP_Freelancing.Models.Entities
{
    /// <summary>
    /// Skill entity (e.g., C#, React, Python)
    /// </summary>
    public class Skill : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        // Navigation properties
        public ICollection<FreelancerSkill> FreelancerSkills { get; set; } = new List<FreelancerSkill>();
        public ICollection<ProjectSkill> ProjectSkills { get; set; } = new List<ProjectSkill>();
    }
}
