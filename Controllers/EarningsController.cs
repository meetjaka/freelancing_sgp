using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SGP_Freelancing.Controllers
{
    [Authorize]
    public class EarningsController : Controller
    {
        private readonly ILogger<EarningsController> _logger;

        public EarningsController(ILogger<EarningsController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.Identity?.Name ?? "User";
            
            ViewBag.UserName = userName;
            ViewBag.UserId = userId;
            
            return View();
        }
    }
}
