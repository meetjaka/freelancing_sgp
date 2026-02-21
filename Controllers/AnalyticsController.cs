using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGP_Freelancing.Data;
using SGP_Freelancing.Models.Entities;
using SGP_Freelancing.Models.ViewModels;
using System.Security.Claims;

namespace SGP_Freelancing.Controllers
{
    [Authorize]
    public class AnalyticsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(ApplicationDbContext context, ILogger<AnalyticsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userName = User.Identity?.Name ?? "User";
                var isClient = User.IsInRole("Client");
                
                ViewBag.UserName = userName;
                ViewBag.UserId = userId;

                var viewModel = new AnalyticsViewModel
                {
                    IsClient = isClient,
                    TopSkills = new List<SkillProgressDto>(),
                    RecentActivities = new List<ActivityDto>()
                };

                // Fake data for profile views
                viewModel.ProfileViews = new Random().Next(50, 500);

                if (isClient)
                {
                    var projects = await _context.Projects
                        .Include(p => p.Bids)
                        .Where(p => p.ClientId == userId && !p.IsDeleted)
                        .ToListAsync();

                    var contracts = await _context.Contracts
                        .Where(c => c.ClientId == userId && !c.IsDeleted)
                        .ToListAsync();

                    viewModel.DealsClosed = contracts.Count;
                    
                    var totalProjects = projects.Count;
                    viewModel.SuccessRate = totalProjects > 0 ? (decimal)viewModel.DealsClosed / totalProjects * 100 : 0;

                    var reviews = await _context.Reviews
                        .Where(r => r.RevieweeId == userId && !r.IsDeleted)
                        .ToListAsync();

                    viewModel.TotalReviews = reviews.Count;
                    viewModel.AverageRating = reviews.Any() ? (decimal)reviews.Average(r => r.Rating) : 0;

                    var topCategories = projects
                        .GroupBy(p => p.CategoryId)
                        .OrderByDescending(g => g.Count())
                        .Take(4)
                        .ToList();

                    var categoryIds = topCategories.Select(c => c.Key).ToList();
                    var categoryNames = await _context.Categories.Where(c => categoryIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id, c => c.Name);

                    var colors = new[] { "indigo-600", "emerald-500", "amber-500", "blue-500" };
                    int colorIndex = 0;

                    foreach (var tc in topCategories)
                    {
                        var percentage = (int)((decimal)tc.Count() / Math.Max(1, projects.Count) * 100);
                        viewModel.TopSkills.Add(new SkillProgressDto
                        {
                            SkillName = categoryNames.GetValueOrDefault(tc.Key, "Other"),
                            Percentage = percentage,
                            ColorClass = colors[colorIndex++ % colors.Length]
                        });
                    }

                    // Recent activities
                    var recentProjects = projects.OrderByDescending(p => p.CreatedAt).Take(5).Select(p => new ActivityDto
                    {
                        ActionName = "Project Posted",
                        ProjectTitle = p.Title,
                        Date = p.CreatedAt,
                        Status = p.Status.ToString(),
                        IconClass = "M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z",
                        ColorClass = "indigo"
                    });
                    
                    viewModel.RecentActivities.AddRange(recentProjects);
                }
                else
                {
                    // Freelancer
                    var bids = await _context.Bids
                        .Include(b => b.Project)
                        .Where(b => b.FreelancerId == userId && !b.IsDeleted)
                        .ToListAsync();

                    var contracts = await _context.Contracts
                        .Where(c => c.FreelancerId == userId && !c.IsDeleted)
                        .ToListAsync();

                    viewModel.DealsClosed = contracts.Count;
                    var totalBids = bids.Count;
                    viewModel.SuccessRate = totalBids > 0 ? (decimal)viewModel.DealsClosed / totalBids * 100 : 0;

                    var reviews = await _context.Reviews
                        .Where(r => r.RevieweeId == userId && !r.IsDeleted)
                        .ToListAsync();

                    viewModel.TotalReviews = reviews.Count;
                    viewModel.AverageRating = reviews.Any() ? (decimal)reviews.Average(r => r.Rating) : 0;

                    // Top skills from won contracts' projects
                    var contractProjectIds = contracts.Select(c => c.ProjectId).ToList();
                    var wonProjects = await _context.Projects
                        .Include(p => p.Category)
                        .Where(p => contractProjectIds.Contains(p.Id))
                        .ToListAsync();

                    var topCategories = wonProjects
                        .GroupBy(p => p.CategoryId)
                        .OrderByDescending(g => g.Count())
                        .Take(4)
                        .ToList();

                    var colors = new[] { "indigo-600", "emerald-500", "amber-500", "blue-500" };
                    int colorIndex = 0;

                    foreach (var tc in topCategories)
                    {
                        var percentage = (int)((decimal)tc.Count() / Math.Max(1, wonProjects.Count) * 100);
                        var categoryName = wonProjects.First(p => p.CategoryId == tc.Key).Category.Name;
                        viewModel.TopSkills.Add(new SkillProgressDto
                        {
                            SkillName = categoryName,
                            Percentage = percentage,
                            ColorClass = colors[colorIndex++ % colors.Length]
                        });
                    }

                    // Recent activities
                    var recentBids = bids.OrderByDescending(b => b.CreatedAt).Take(5).Select(b => new ActivityDto
                    {
                        ActionName = "Proposal Submitted",
                        ProjectTitle = b.Project.Title,
                        Date = b.CreatedAt,
                        Status = b.Status.ToString(),
                        IconClass = "M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z",
                        ColorClass = "indigo"
                    });
                    
                    viewModel.RecentActivities.AddRange(recentBids);
                }

                // Add messages to activities for both
                var messages = await _context.Messages
                    .Where(m => m.ReceiverId == userId && !m.IsDeleted)
                    .OrderByDescending(m => m.CreatedAt)
                    .Take(3)
                    .ToListAsync();
                
                foreach (var msg in messages)
                {
                    viewModel.RecentActivities.Add(new ActivityDto
                    {
                        ActionName = "Message Received",
                        ProjectTitle = "Direct Message",
                        Date = msg.CreatedAt,
                        Status = msg.IsRead ? "Read" : "Unread",
                        IconClass = "M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z",
                        ColorClass = "emerald"
                    });
                }

                viewModel.RecentActivities = viewModel.RecentActivities.OrderByDescending(a => a.Date).Take(10).ToList();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading analytics dashboard");
                return View(new AnalyticsViewModel());
            }
        }
    }
}
