using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGP_Freelancing.Models.Entities
{
    /// <summary>
    /// Fiverr-style gig/service listed by freelancers
    /// </summary>
    public class FreelancerService : BaseEntity
    {
        [Required]
        public string FreelancerId { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        [Column(TypeName = "nvarchar(MAX)")]
        public string Description { get; set; } = null!;

        public int CategoryId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int DeliveryDays { get; set; }

        public bool IsActive { get; set; } = true;

        public int TotalOrders { get; set; }

        [Column(TypeName = "decimal(3,2)")]
        public decimal AverageRating { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        // Navigation
        [ForeignKey(nameof(FreelancerId))]
        public ApplicationUser Freelancer { get; set; } = null!;

        [ForeignKey(nameof(CategoryId))]
        public Category Category { get; set; } = null!;

        public ICollection<ServiceOrder> Orders { get; set; } = new List<ServiceOrder>();
        public ICollection<FreelancerServiceSkill> ServiceSkills { get; set; } = new List<FreelancerServiceSkill>();
    }

    /// <summary>
    /// Junction table for FreelancerService ↔ Skill
    /// </summary>
    public class FreelancerServiceSkill
    {
        public int FreelancerServiceId { get; set; }
        public int SkillId { get; set; }

        [ForeignKey(nameof(FreelancerServiceId))]
        public FreelancerService FreelancerService { get; set; } = null!;

        [ForeignKey(nameof(SkillId))]
        public Skill Skill { get; set; } = null!;
    }

    /// <summary>
    /// Order placed on a freelancer service/gig
    /// </summary>
    public class ServiceOrder : BaseEntity
    {
        public int FreelancerServiceId { get; set; }

        [Required]
        public string ClientId { get; set; } = null!;

        [Required]
        public string FreelancerId { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public ServiceOrderStatus Status { get; set; } = ServiceOrderStatus.Placed;

        [Column(TypeName = "nvarchar(MAX)")]
        public string? Requirements { get; set; }

        public DateTime? DeliveryDate { get; set; }

        public DateTime? CompletedAt { get; set; }

        // Navigation
        [ForeignKey(nameof(FreelancerServiceId))]
        public FreelancerService FreelancerService { get; set; } = null!;

        [ForeignKey(nameof(ClientId))]
        public ApplicationUser Client { get; set; } = null!;

        [ForeignKey(nameof(FreelancerId))]
        public ApplicationUser Freelancer { get; set; } = null!;
    }

    public enum ServiceOrderStatus
    {
        Placed = 0,
        InProgress = 1,
        Delivered = 2,
        Completed = 3,
        Cancelled = 4,
        Disputed = 5
    }
}
