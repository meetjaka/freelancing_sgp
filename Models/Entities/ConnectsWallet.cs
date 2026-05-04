using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGP_Freelancing.Models.Entities
{
    /// <summary>
    /// Wallet tracking a freelancer's available connects/tokens for bidding
    /// </summary>
    public class ConnectsWallet : BaseEntity
    {
        [Required]
        public string UserId { get; set; } = null!;

        public int Balance { get; set; } = 10;

        public int MonthlyAllocation { get; set; } = 10;

        public DateTime? LastRefillDate { get; set; }

        // Navigation
        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        public ICollection<ConnectsTransaction> Transactions { get; set; } = new List<ConnectsTransaction>();
    }

    /// <summary>
    /// Ledger of connects spent/earned
    /// </summary>
    public class ConnectsTransaction : BaseEntity
    {
        [Required]
        public string UserId { get; set; } = null!;

        public int WalletId { get; set; }

        public int Amount { get; set; }

        public ConnectsTransactionType Type { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int? BidId { get; set; }

        // Navigation
        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        [ForeignKey(nameof(WalletId))]
        public ConnectsWallet Wallet { get; set; } = null!;
    }

    public enum ConnectsTransactionType
    {
        MonthlyRefill = 0,
        BidSpend = 1,
        Purchase = 2,
        Refund = 3
    }
}
