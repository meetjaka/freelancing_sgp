namespace SGP_Freelancing.Services
{
    /// <summary>
    /// Stores OTP records in-memory (keyed by email).
    /// In production you can swap this for a distributed cache / DB table.
    /// </summary>
    public class OtpService
    {
        private readonly Dictionary<string, OtpRecord> _store = new(StringComparer.OrdinalIgnoreCase);
        private readonly ILogger<OtpService> _logger;

        public OtpService(ILogger<OtpService> logger)
        {
            _logger = logger;
        }

        /// <summary>Generates a 6-digit OTP, stores it, and returns it.</summary>
        public string GenerateOtp(string email)
        {
            var otp = Random.Shared.Next(100000, 999999).ToString();
            _store[email] = new OtpRecord(otp, DateTime.UtcNow.AddMinutes(10));
            _logger.LogInformation("OTP generated for {Email}", email);
            return otp;
        }

        /// <summary>Returns true if the OTP is correct and not expired.</summary>
        public bool VerifyOtp(string email, string otp)
        {
            if (!_store.TryGetValue(email, out var record))
                return false;

            if (record.ExpiresAt < DateTime.UtcNow)
            {
                _store.Remove(email);
                return false;
            }

            if (!string.Equals(record.Otp, otp, StringComparison.Ordinal))
                return false;

            _store.Remove(email); // one-time use
            return true;
        }

        private record OtpRecord(string Otp, DateTime ExpiresAt);
    }
}
