using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGP_Freelancing.Models.Entities
{
    /// <summary>
    /// Message between users
    /// </summary>
    public class Message : BaseEntity
    {
        [Required]
        public string SenderId { get; set; } = null!;
        
        [Required]
        public string ReceiverId { get; set; } = null!;
        
        [Required]
        [MaxLength(200)]
        public string Subject { get; set; } = null!;
        
        [Required]
        [Column(TypeName = "nvarchar(MAX)")]
        public string Content { get; set; } = null!;
        
        public bool IsRead { get; set; } = false;
        
        public DateTime? ReadAt { get; set; }
        
        // Navigation properties
        [ForeignKey(nameof(SenderId))]
        public ApplicationUser Sender { get; set; } = null!;
        
        [ForeignKey(nameof(ReceiverId))]
        public ApplicationUser Receiver { get; set; } = null!;
    }
}
