using System.ComponentModel.DataAnnotations.Schema;

namespace SGP_Freelancing.Models.Entities
{
    /// <summary>
    /// Many-to-many relationship between Freelancer and Skill
    /// </summary>
    public class FreelancerSkill
    {
        public int FreelancerProfileId { get; set; }
        public int SkillId { get; set; }
        public int YearsOfExperience { get; set; }
        
        // Navigation properties
        [ForeignKey(nameof(FreelancerProfileId))]
        public FreelancerProfile FreelancerProfile { get; set; } = null!;
        
        [ForeignKey(nameof(SkillId))]
        public Skill Skill { get; set; } = null!;
    }
}
