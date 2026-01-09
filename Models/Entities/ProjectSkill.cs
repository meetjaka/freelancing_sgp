using System.ComponentModel.DataAnnotations.Schema;

namespace SGP_Freelancing.Models.Entities
{
    /// <summary>
    /// Many-to-many relationship between Project and Skill
    /// </summary>
    public class ProjectSkill
    {
        public int ProjectId { get; set; }
        public int SkillId { get; set; }
        
        // Navigation properties
        [ForeignKey(nameof(ProjectId))]
        public Project Project { get; set; } = null!;
        
        [ForeignKey(nameof(SkillId))]
        public Skill Skill { get; set; } = null!;
    }
}
