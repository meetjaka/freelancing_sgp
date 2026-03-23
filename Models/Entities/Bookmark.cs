using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGP_Freelancing.Models.Entities
{
    /// <summary>
    /// Bookmark/Save entity for projects and freelancers
    /// </summary>
    public class Bookmark : BaseEntity
    {
        [Required]
        public string UserId { get; set; } = null!;
        
        /// <summary>
        /// Type of bookmark: "Project" or "Freelancer"
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string BookmarkType { get; set; } = null!;
        
        /// <summary>
        /// The ID of the bookmarked item (ProjectId or FreelancerProfileId)
        /// </summary>
        public int ItemId { get; set; }
        
        /// <summary>
        /// Optional note by the user
        /// </summary>
        [MaxLength(500)]
        public string? Note { get; set; }
        
        // Navigation property
        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;
    }
    
    public static class BookmarkTypes
    {
        public const string Project = "Project";
        public const string Freelancer = "Freelancer";
    }
}
