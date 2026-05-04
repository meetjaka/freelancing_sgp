using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGP_Freelancing.Models.Entities
{
    /// <summary>
    /// Payment transaction for contracts
    /// </summary>
    public class PaymentTransaction : BaseEntity
    {
        public int ContractId { get; set; }
        
        public int? MilestoneId { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        
        public PaymentType Type { get; set; }
        
        public string? TransactionId { get; set; }
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public DateTime? ProcessedAt { get; set; }
        
        // Navigation properties
        [ForeignKey(nameof(ContractId))]
        public Contract Contract { get; set; } = null!;
        
        [ForeignKey(nameof(MilestoneId))]
        public Milestone? Milestone { get; set; }
    }
    
    public enum PaymentStatus
    {
        Pending = 0,
        Processing = 1,
        Completed = 2,
        Failed = 3,
        Refunded = 4,
        Escrow = 5,
        Released = 6
    }
    
    public enum PaymentType
    {
        Deposit = 0,
        Milestone = 1,
        Final = 2,
        Refund = 3,
        EscrowDeposit = 4,
        EscrowRelease = 5
    }
}
