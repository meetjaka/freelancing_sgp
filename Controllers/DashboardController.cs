using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGP_Freelancing.Services.Interfaces;
using System.Security.Claims;

namespace SGP_Freelancing.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IProjectService _projectService;
        private readonly IBidService _bidService;
        private readonly IMessageService _messageService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IProjectService projectService,
            IBidService bidService,
            IMessageService messageService,
            ILogger<DashboardController> logger)
        {
            _projectService = projectService;
            _bidService = bidService;
            _messageService = messageService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isFreelancer = User.IsInRole("Freelancer");
                var isClient = User.IsInRole("Client");

                // Get statistics
                var projects = await _projectService.GetAllProjectsAsync(1, 100);
                
                // Calculate stats (showing all projects for demo)
                var totalProjects = projects.Items.Count;
                var activeProjects = projects.Items.Count(p => p.Status == "Open" || p.Status == "InProgress");
                var completedProjects = projects.Items.Count(p => p.Status == "Completed");
                
                // Get recent messages count
                var unreadMessages = 5; // You can implement actual count

                ViewBag.TotalProjects = totalProjects;
                ViewBag.ActiveProjects = activeProjects;
                ViewBag.CompletedProjects = completedProjects;
                ViewBag.UnreadMessages = unreadMessages;
                ViewBag.IsFreelancer = isFreelancer;
                ViewBag.IsClient = isClient;
                ViewBag.UserName = User.Identity.Name;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                return View();
            }
        }
    }
}
