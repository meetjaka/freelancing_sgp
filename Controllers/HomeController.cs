using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SGP_Freelancing.Models;
using SGP_Freelancing.Repositories;
using SGP_Freelancing.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace SGP_Freelancing.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IUnitOfWork unitOfWork, ILogger<HomeController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var totalProjects = await _unitOfWork.Projects.CountAsync(p => !p.IsDeleted);
            var totalFreelancers = await _unitOfWork.FreelancerProfiles.CountAsync();
            var totalClients = await _unitOfWork.ClientProfiles.CountAsync();

            var latestProject = await _unitOfWork.Projects.Query()
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync(p => !p.IsDeleted);

            var latestContract = await _unitOfWork.Contracts.Query()
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync(c => !c.IsDeleted);

            ViewBag.TotalProjects = totalProjects;
            ViewBag.TotalFreelancers = totalFreelancers;
            ViewBag.TotalClients = totalClients;
            ViewBag.LatestProjectTitle = latestProject?.Title ?? "New project";
            ViewBag.LatestProjectBudget = latestProject?.Budget ?? 0m;
            ViewBag.LatestProjectCategory = latestProject?.Category?.Name ?? "Projects";
            ViewBag.LatestProjectStatus = latestProject?.Status.ToString() ?? "Open";
            ViewBag.LatestContractAmount = latestContract?.AgreedAmount ?? 0m;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult TailwindTest()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
