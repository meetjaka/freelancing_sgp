using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SGP_Freelancing.Models.Entities;
using SGP_Freelancing.Services;
using SGP_Freelancing.Services.Interfaces;

namespace SGP_Freelancing.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IEmailService _emailService;
        private readonly OtpService _otpService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger,
            IEmailService emailService,
            OtpService otpService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailService = emailService;
            _otpService = otpService;
        }

        // ─────────────────────────────────────────────────────────────
        // LOGIN
        // ─────────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe, string returnUrl = null)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Email and password are required");
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View();
            }

            // Check if email is confirmed
            if (!user.EmailConfirmed)
            {
                // Re-send OTP and redirect to verification
                var otp = _otpService.GenerateOtp(email);
                var userName = $"{user.FirstName} {user.LastName}";
                try
                {
                    await _emailService.SendOtpEmailAsync(email, userName, otp);
                    TempData["InfoMessage"] = "Your email is not verified. A new OTP has been sent to your email.";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to resend OTP during login for {Email}", email);
                    TempData["InfoMessage"] = "Your email is not verified. Please check your inbox or request a new OTP.";
                }
                return RedirectToAction("VerifyOtp", new { email });
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName ?? string.Empty, password, rememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Dashboard");
            }

            ModelState.AddModelError("", "Invalid email or password");
            return View();
        }

        // ─────────────────────────────────────────────────────────────
        // REGISTER
        // ─────────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string firstName, string lastName, string email, string password, string confirmPassword, string Role)
        {
            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match");
                return View();
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = false   // will be confirmed after OTP
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                // Add user to role
                if (!string.IsNullOrEmpty(Role))
                    await _userManager.AddToRoleAsync(user, Role);

                _logger.LogInformation("User created a new account with password.");

                // Generate OTP (saved to DB)
                var otp = _otpService.GenerateOtp(email);

                // Send OTP email – awaited so it completes before redirect (required on Render)
                try
                {
                    await _emailService.SendOtpEmailAsync(email, $"{firstName} {lastName}", otp);
                    _logger.LogInformation("OTP email sent to {Email}", email);
                    TempData["SuccessMessage"] = $"Account created! An OTP has been sent to {email}. Please verify your email to continue.";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send OTP email to {Email}", email);
                    TempData["SuccessMessage"] = $"Account created! However, we couldn't send the OTP email. Please use Resend OTP on the next page.";
                }
                return RedirectToAction("VerifyOtp", new { email });
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View();
        }

        // ─────────────────────────────────────────────────────────────
        // VERIFY OTP
        // ─────────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult VerifyOtp(string email)
        {
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Register");

            ViewData["Email"] = email;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(string email, string otp)
        {
            ViewData["Email"] = email;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otp))
            {
                ModelState.AddModelError("", "Email and OTP are required.");
                return View();
            }

            if (!_otpService.VerifyOtp(email, otp.Trim()))
            {
                ModelState.AddModelError("", "Invalid or expired OTP. Please try again.");
                return View();
            }

            // Mark email as confirmed
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return View();
            }

            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            // Sign the user in
            await _signInManager.SignInAsync(user, isPersistent: false);
            _logger.LogInformation("User {Email} verified OTP and signed in.", email);

            TempData["SuccessMessage"] = "Email verified successfully! Welcome to SGP Freelancing.";
            return RedirectToAction("Index", "Dashboard");
        }

        // ─────────────────────────────────────────────────────────────
        // RESEND OTP
        // ─────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendOtp(string email)
        {
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Register");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "No account found with this email.";
                return RedirectToAction("Register");
            }

            var otp = _otpService.GenerateOtp(email);
            var resendUserName = $"{user.FirstName} {user.LastName}";
            try
            {
                await _emailService.SendOtpEmailAsync(email, resendUserName, otp);
                _logger.LogInformation("Resend OTP email sent to {Email}", email);
                TempData["SuccessMessage"] = "A new OTP has been sent to your email.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resend OTP to {Email}", email);
                TempData["ErrorMessage"] = "Failed to send OTP email. Please try again or contact support.";
            }

            return RedirectToAction("VerifyOtp", new { email });
        }

        // ─────────────────────────────────────────────────────────────
        // LOGOUT
        // ─────────────────────────────────────────────────────────────
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Index", "Home");
        }

        // ─────────────────────────────────────────────────────────────
        // PROFILE / ACCESS DENIED
        // ─────────────────────────────────────────────────────────────
        [Authorize]
        public IActionResult Profile()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Dashboard");
        }
    }
}
