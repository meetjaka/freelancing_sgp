namespace SGP_Freelancing.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string toEmail, string toName, string otp);
        Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
    }
}
