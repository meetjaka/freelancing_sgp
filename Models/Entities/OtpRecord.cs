namespace SGP_Freelancing.Models.Entities
{
    /// <summary>
    /// Stores OTP records in the database so they survive server restarts
    /// and work correctly across multiple instances (e.g., on Render).
    /// </summary>
    public class OtpRecord
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
