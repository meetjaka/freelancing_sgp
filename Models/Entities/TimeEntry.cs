using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGP_Freelancing.Models.Entities
{
    /// <summary>
    /// Time entry for hourly contract tracking
    /// </summary>
    public class TimeEntry : BaseEntity
    {
        public int ContractId { get; set; }

        [Required]
        public string FreelancerId { get; set; } = null!;

        public DateTime Date { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal HoursWorked { get; set; }

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = null!;

        public TimeEntryStatus Status { get; set; } = TimeEntryStatus.Logged;

        public string? ApprovedByClientId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        // Navigation
        [ForeignKey(nameof(ContractId))]
        public Contract Contract { get; set; } = null!;

        [ForeignKey(nameof(FreelancerId))]
        public ApplicationUser Freelancer { get; set; } = null!;

        [ForeignKey(nameof(ApprovedByClientId))]
        public ApplicationUser? ApprovedByClient { get; set; }
    }

    public enum TimeEntryStatus
    {
        Logged = 0,
        Approved = 1,
        Rejected = 2
    }
}
