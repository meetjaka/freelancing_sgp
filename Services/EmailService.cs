using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SendGrid;
using SendGrid.Helpers.Mail;
using SGP_Freelancing.Services.Interfaces;

namespace SGP_Freelancing.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendOtpEmailAsync(string toEmail, string toName, string otp)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");

            // Priority 1: SendGrid API key (env var SENDGRID_API_KEY or config)
            var sendGridKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY")
                           ?? emailSettings["SendGridApiKey"]
                           ?? _configuration["SendGridApiKey"];

            if (!string.IsNullOrWhiteSpace(sendGridKey))
            {
                _logger.LogInformation("Using SendGrid to send OTP email to {Email}", toEmail);
                await SendViaSendGridAsync(sendGridKey, emailSettings, toEmail, toName, otp);
                return;
            }

            // Priority 2: SMTP (works locally; may be blocked on Render)
            _logger.LogWarning("No SendGrid API key found. Falling back to SMTP for {Email}. NOTE: SMTP is often blocked on Render/cloud platforms.", toEmail);
            await SendViaSmtpAsync(emailSettings, toEmail, toName, otp);
        }

        // ── SendGrid (HTTPS API – works on Render and all cloud platforms) ─────
        private async Task SendViaSendGridAsync(
            string apiKey,
            IConfigurationSection emailSettings,
            string toEmail, string toName, string otp)
        {
            var client = new SendGridClient(apiKey);

            var senderEmail = emailSettings["SenderEmail"] ?? "meetjakasaniya24@gmail.com";
            var senderName  = emailSettings["SenderName"]  ?? "SGP Freelancing";

            var from    = new EmailAddress(senderEmail, senderName);
            var to      = new EmailAddress(toEmail, toName);
            var subject = "Verify Your Email – SGP Freelancing";
            var html    = BuildOtpEmailHtml(toName, otp);

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent: null, htmlContent: html);

            var response = await client.SendEmailAsync(msg);

            if ((int)response.StatusCode >= 200 && (int)response.StatusCode < 300)
            {
                _logger.LogInformation("✅ OTP email sent via SendGrid to {Email} (status {Status})", toEmail, response.StatusCode);
            }
            else
            {
                var body = await response.Body.ReadAsStringAsync();
                _logger.LogError("❌ SendGrid failed for {Email}: {Status} – {Body}", toEmail, response.StatusCode, body);
                throw new Exception($"SendGrid returned {response.StatusCode}: {body}");
            }
        }

        // ── SMTP (MailKit – works locally; blocked on Render) ───────────────────
        private async Task SendViaSmtpAsync(
            IConfigurationSection emailSettings,
            string toEmail, string toName, string otp)
        {
            var host     = emailSettings["SmtpHost"]     ?? "smtp.gmail.com";
            var port     = int.Parse(emailSettings["SmtpPort"] ?? "587");
            var useSsl   = bool.Parse(emailSettings["UseSsl"]  ?? "false");
            var username = emailSettings["SmtpUsername"] ?? "";
            var password = emailSettings["SmtpPassword"] ?? "";

            _logger.LogInformation("Attempting SMTP connection to {Host}:{Port} (SSL={SSL}) for {Email}", host, port, useSsl, toEmail);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                emailSettings["SenderName"]  ?? "SGP Freelancing",
                emailSettings["SenderEmail"] ?? "noreply@sgpfreelancing.com"
            ));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = "Verify Your Email – SGP Freelancing";

            var bodyBuilder = new BodyBuilder { HtmlBody = BuildOtpEmailHtml(toName, otp) };
            message.Body = bodyBuilder.ToMessageBody();

            // Port 465 = SSL, Port 587 = StartTLS
            var secureOption = (port == 465 || useSsl)
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTls;

            try
            {
                using var smtpClient = new SmtpClient();
                smtpClient.Timeout = 20000; // 20 seconds

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                await smtpClient.ConnectAsync(host, port, secureOption, cts.Token);
                _logger.LogInformation("SMTP connected to {Host}:{Port}", host, port);

                await smtpClient.AuthenticateAsync(username, password, cts.Token);
                _logger.LogInformation("SMTP authenticated as {Username}", username);

                await smtpClient.SendAsync(message, cancellationToken: cts.Token);
                await smtpClient.DisconnectAsync(true, cts.Token);

                _logger.LogInformation("✅ OTP email sent via SMTP to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ SMTP failed for {Email} (Host={Host}, Port={Port}). " +
                    "On Render/cloud platforms, SMTP port 587/465 is often blocked. " +
                    "Please set the SENDGRID_API_KEY environment variable on Render.", toEmail, host, port);
                throw;
            }
        }

        // ── HTML Template ────────────────────────────────────────────────────────
        private static string BuildOtpEmailHtml(string name, string otp)
        {
            return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0""/>
  <title>Email Verification</title>
</head>
<body style=""margin:0;padding:0;background:#0f172a;font-family:'Segoe UI',Arial,sans-serif;"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#0f172a;padding:40px 0;"">
    <tr>
      <td align=""center"">
        <table width=""520"" cellpadding=""0"" cellspacing=""0"" style=""background:linear-gradient(135deg,#1e293b,#0f172a);border-radius:20px;border:1px solid #334155;overflow:hidden;"">
          <!-- Header -->
          <tr>
            <td style=""background:linear-gradient(135deg,#4f46e5,#7c3aed);padding:36px 40px;text-align:center;"">
              <div style=""display:inline-block;background:rgba(255,255,255,0.15);border-radius:14px;padding:12px 20px;margin-bottom:16px;"">
                <span style=""font-size:28px;"">🔐</span>
              </div>
              <h1 style=""margin:0;color:#ffffff;font-size:26px;font-weight:700;letter-spacing:-0.5px;"">Verify Your Email</h1>
              <p style=""margin:8px 0 0;color:rgba(255,255,255,0.75);font-size:14px;"">SGP Freelancing Platform</p>
            </td>
          </tr>
          <!-- Body -->
          <tr>
            <td style=""padding:40px;"">
              <p style=""margin:0 0 20px;color:#cbd5e1;font-size:16px;"">Hi <strong style=""color:#e2e8f0;"">{name}</strong>,</p>
              <p style=""margin:0 0 28px;color:#94a3b8;font-size:15px;line-height:1.6;"">
                Welcome to SGP Freelancing! Use the OTP below to verify your email address and activate your account.
              </p>

              <!-- OTP Box -->
              <div style=""background:linear-gradient(135deg,#1e293b,#0f172a);border:2px solid #4f46e5;border-radius:16px;padding:32px;text-align:center;margin-bottom:28px;"">
                <p style=""margin:0 0 8px;color:#94a3b8;font-size:13px;text-transform:uppercase;letter-spacing:2px;"">Your One-Time Password</p>
                <div style=""letter-spacing:16px;font-size:42px;font-weight:800;color:#818cf8;font-family:monospace;margin:8px 0;"">{otp}</div>
                <p style=""margin:12px 0 0;color:#64748b;font-size:13px;"">⏱ Valid for <strong style=""color:#94a3b8;"">10 minutes</strong></p>
              </div>

              <p style=""margin:0 0 8px;color:#64748b;font-size:13px;text-align:center;"">
                If you did not create an account, please ignore this email.
              </p>
            </td>
          </tr>
          <!-- Footer -->
          <tr>
            <td style=""background:#0f172a;border-top:1px solid #1e293b;padding:24px 40px;text-align:center;"">
              <p style=""margin:0;color:#475569;font-size:12px;"">© 2025 SGP Freelancing · All rights reserved</p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
        }
    }
}
