using Microsoft.EntityFrameworkCore;
using SGP_Freelancing.Data;
using SGP_Freelancing.Models.Entities;

namespace SGP_Freelancing.Services
{
    /// <summary>
    /// Stores OTP records in the DATABASE so they survive server restarts
    /// and work correctly on Render (no in-memory state issues).
    /// </summary>
    public class OtpService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OtpService> _logger;

        public OtpService(IServiceScopeFactory scopeFactory, ILogger<OtpService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        /// <summary>Generates a 6-digit OTP, saves it to DB, and returns it.</summary>
        public string GenerateOtp(string email)
        {
            var otp = Random.Shared.Next(100000, 999999).ToString();

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Remove any existing OTPs for this email
            var existing = db.OtpRecords.Where(r => r.Email == email).ToList();
            db.OtpRecords.RemoveRange(existing);

            db.OtpRecords.Add(new OtpRecord
            {
                Email = email,
                Otp = otp,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                CreatedAt = DateTime.UtcNow
            });

            db.SaveChanges();

            _logger.LogInformation("OTP generated and saved to DB for {Email}", email);
            return otp;
        }

        /// <summary>Returns true if the OTP is correct and not expired.</summary>
        public bool VerifyOtp(string email, string otp)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var record = db.OtpRecords
                .Where(r => r.Email == email)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefault();

            if (record == null)
            {
                _logger.LogWarning("OTP verification failed: no record found for {Email}", email);
                return false;
            }

            if (record.ExpiresAt < DateTime.UtcNow)
            {
                db.OtpRecords.Remove(record);
                db.SaveChanges();
                _logger.LogWarning("OTP expired for {Email}", email);
                return false;
            }

            if (!string.Equals(record.Otp, otp, StringComparison.Ordinal))
            {
                _logger.LogWarning("OTP mismatch for {Email}", email);
                return false;
            }

            // One-time use: remove after successful verification
            db.OtpRecords.Remove(record);
            db.SaveChanges();

            _logger.LogInformation("OTP verified successfully for {Email}", email);
            return true;
        }
    }
}
