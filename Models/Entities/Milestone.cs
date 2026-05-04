using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGP_Freelancing.Models.Entities
{
    public class Milestone : BaseEntity
    {
        public int ContractId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [Column(TypeName = "nvarchar(MAX)")]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime? DueDate { get; set; }

        public int Order { get; set; }

        public MilestoneStatus Status { get; set; } = MilestoneStatus.Pending;

        // Navigation
        [ForeignKey(nameof(ContractId))]
        public Contract Contract { get; set; } = null!;

        public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
    }

    public enum MilestoneStatus
    {
        Pending = 0,
        Funded = 1,
        InProgress = 2,
        Submitted = 3,
        Approved = 4,
        Disputed = 5
    }
}
