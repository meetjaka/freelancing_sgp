using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGP_Freelancing.Models.Entities
{
    /// <summary>
    /// File attachment for projects, contracts, messages
    /// </summary>
    public class FileAttachment : BaseEntity
    {
        [Required]
        [MaxLength(500)]
        public string FileName { get; set; } = null!;

        [Required]
        [MaxLength(1000)]
        public string FilePath { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string FileType { get; set; } = null!;
        
        public long FileSize { get; set; } // in bytes

        [Required]
        public string UploadedById { get; set; } = null!;

        // Optional: Link to specific entities
        public int? ProjectId { get; set; }
        public int? ContractId { get; set; }
        public int? MessageId { get; set; }

        // Navigation properties
        [ForeignKey(nameof(UploadedById))]
        public ApplicationUser UploadedBy { get; set; } = null!;

        [ForeignKey(nameof(ProjectId))]
        public Project? Project { get; set; }

        [ForeignKey(nameof(ContractId))]
        public Contract? Contract { get; set; }

        [ForeignKey(nameof(MessageId))]
        public Message? Message { get; set; }
    }
}
