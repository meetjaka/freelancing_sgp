using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGP_Freelancing.Models.Entities
{
    /// <summary>
    /// Contract between client and freelancer
    /// </summary>
    public class Contract : BaseEntity
    {
        public int ProjectId { get; set; }
        
        [Required]
        public string ClientId { get; set; } = null!;
        
        [Required]
        public string FreelancerId { get; set; } = null!;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal AgreedAmount { get; set; }
        
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        
        [Column(TypeName = "nvarchar(MAX)")]
        public string? Terms { get; set; }
        
        public ContractStatus Status { get; set; } = ContractStatus.Active;
        
        // Navigation properties
        [ForeignKey(nameof(ProjectId))]
        public Project Project { get; set; } = null!;
        
        [ForeignKey(nameof(ClientId))]
        public ApplicationUser Client { get; set; } = null!;
        
        [ForeignKey(nameof(FreelancerId))]
        public ApplicationUser Freelancer { get; set; } = null!;
        
        public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
    }
    
    public enum ContractStatus
    {
        Active = 0,
        Completed = 1,
        Cancelled = 2,
        Disputed = 3
    }
}
