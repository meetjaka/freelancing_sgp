using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGP_Freelancing.Models.Entities
{
    /// <summary>
    /// Dispute raised on a contract for admin mediation
    /// </summary>
    public class Dispute : BaseEntity
    {
        public int ContractId { get; set; }

        [Required]
        public string RaisedByUserId { get; set; } = null!;

        [Required]
        [Column(TypeName = "nvarchar(MAX)")]
        public string Reason { get; set; } = null!;

        public DisputeStatus Status { get; set; } = DisputeStatus.Open;

        [Column(TypeName = "nvarchar(MAX)")]
        public string? Resolution { get; set; }

        public string? ResolvedByAdminId { get; set; }

        public DateTime? ResolvedAt { get; set; }

        // Navigation
        [ForeignKey(nameof(ContractId))]
        public Contract Contract { get; set; } = null!;

        [ForeignKey(nameof(RaisedByUserId))]
        public ApplicationUser RaisedByUser { get; set; } = null!;

        [ForeignKey(nameof(ResolvedByAdminId))]
        public ApplicationUser? ResolvedByAdmin { get; set; }
    }

    public enum DisputeStatus
    {
        Open = 0,
        UnderReview = 1,
        Resolved = 2,
        Closed = 3
    }
}
