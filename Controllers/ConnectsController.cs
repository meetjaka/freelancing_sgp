using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGP_Freelancing.Models.ViewModels;
using SGP_Freelancing.Services.Interfaces;
using System.Security.Claims;

namespace SGP_Freelancing.Controllers
{
    [Authorize(Roles = "Freelancer")]
    public class ConnectsController : Controller
    {
        private readonly IConnectsService _connectsService;
        private readonly ILogger<ConnectsController> _logger;

        public ConnectsController(IConnectsService connectsService, ILogger<ConnectsController> logger)
        {
            _connectsService = connectsService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var wallet = await _connectsService.GetWalletAsync(userId);
            var vm = new ConnectsViewModel { Wallet = wallet };
            return View(vm);
        }
    }
}
