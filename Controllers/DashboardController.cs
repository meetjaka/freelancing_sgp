using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Models.Entities;
using SGP_Freelancing.Models.ViewModels;
using SGP_Freelancing.Repositories;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IVerificationService _verificationService;
        private readonly IDisputeService _disputeService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IProjectService projectService,
            IBidService bidService,
            IMessageService messageService,
            IUnitOfWork unitOfWork,
            IVerificationService verificationService,
            IDisputeService disputeService,
            UserManager<ApplicationUser> userManager,
            ILogger<DashboardController> logger)
        {
            _projectService = projectService;
            _bidService = bidService;
            _messageService = messageService;
            _unitOfWork = unitOfWork;
            _verificationService = verificationService;
            _disputeService = disputeService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                if (User.IsInRole("Admin"))
                {
                    var pendingVerifications = await _verificationService.GetPendingVerificationsAsync();
                    var pendingDisputes = await _disputeService.GetAllPendingDisputesAsync();
                    var totalUsers = await _userManager.Users.CountAsync();
                    var totalOpenProjects = await _unitOfWork.Projects.CountAsync(p => !p.IsDeleted && p.Status == ProjectStatus.Open);

                    var adminVm = new AdminHubViewModel
                    {
                        PendingVerificationCount = pendingVerifications.Count,
                        PendingDisputeCount = pendingDisputes.Count,
                        TotalRegisteredUsers = totalUsers,
                        TotalOpenProjects = totalOpenProjects
                    };
                    ViewBag.UserName = User.Identity?.Name;
                    return View("Admin", adminVm);
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isFreelancer = User.IsInRole("Freelancer");
                var isClient = User.IsInRole("Client");

                var recentProjects = new List<ProjectDto>();
                var recentActivities = new List<ActivityDto>();

                if (isClient && !string.IsNullOrWhiteSpace(userId))
                {
                    recentProjects = await _projectService.GetProjectsByClientAsync(userId);
                }
                else if (isFreelancer && !string.IsNullOrWhiteSpace(userId))
                {
                    recentProjects = await _projectService.GetRecommendedProjectsAsync(userId, 5);
                }

                var totalProjects = isClient && !string.IsNullOrWhiteSpace(userId)
                    ? recentProjects.Count
                    : await _unitOfWork.Projects.CountAsync(p => !p.IsDeleted && p.Status == ProjectStatus.Open);

                var activeProjects = recentProjects.Count(p => p.Status == "Open" || p.Status == "InProgress");
                var completedProjects = recentProjects.Count(p => p.Status == "Completed");
                var unreadMessages = !string.IsNullOrWhiteSpace(userId)
                    ? await _messageService.GetUnreadCountAsync(userId)
                    : 0;

                if (isClient && !string.IsNullOrWhiteSpace(userId))
                {
                    recentActivities = await _unitOfWork.Repository<Project>()
                        .Query()
                        .Where(p => p.ClientId == userId && !p.IsDeleted)
                        .OrderByDescending(p => p.CreatedAt)
                        .Take(4)
                        .Select(p => new ActivityDto
                        {
                            ActionName = "Project Posted",
                            ProjectTitle = p.Title,
                            Date = p.CreatedAt,
                            Status = p.Status.ToString(),
                            IconClass = "M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z",
                            ColorClass = "indigo"
                        })
                        .ToListAsync();

                    var recentProjectIds = recentProjects.Take(3).Select(p => p.Id).ToList();
                    if (recentProjectIds.Any())
                    {
                        recentActivities.AddRange(await _unitOfWork.Bids
                            .Query()
                            .Include(b => b.Project)
                            .Where(b => recentProjectIds.Contains(b.ProjectId) && !b.IsDeleted)
                            .OrderByDescending(b => b.CreatedAt)
                            .Take(3)
                            .Select(b => new ActivityDto
                            {
                                ActionName = b.Status == BidStatus.Accepted ? "Bid Accepted" : "Proposal Received",
                                ProjectTitle = b.Project.Title,
                                Date = b.CreatedAt,
                                Status = b.Status.ToString(),
                                IconClass = "M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z",
                                ColorClass = "emerald"
                            })
                            .ToListAsync());

                        var clientMessages = await _unitOfWork.Repository<Message>()
                            .Query()
                            .Include(m => m.Sender)
                            .Include(m => m.Receiver)
                            .Where(m => (m.SenderId == userId || m.ReceiverId == userId) && !m.IsDeleted && m.Subject != "SYSTEM_CONVERSATION_CLOSED")
                            .OrderByDescending(m => m.CreatedAt)
                            .Take(4)
                            .ToListAsync();

                        recentActivities.AddRange(clientMessages.Select(m => new ActivityDto
                        {
                            ActionName = m.SenderId == userId ? "Message Sent" : "Message Received",
                            ProjectTitle = m.SenderId == userId
                                ? $"To {m.Receiver.FirstName} {m.Receiver.LastName}"
                                : $"From {m.Sender.FirstName} {m.Sender.LastName}",
                            Date = m.CreatedAt,
                            Status = m.IsRead ? "Read" : "Unread",
                            IconClass = "M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z",
                            ColorClass = "emerald"
                        }));
                    }
                }
                else if (isFreelancer && !string.IsNullOrWhiteSpace(userId))
                {
                    recentActivities = await _unitOfWork.Bids
                        .Query()
                        .Include(b => b.Project)
                        .Where(b => b.FreelancerId == userId && !b.IsDeleted)
                        .OrderByDescending(b => b.CreatedAt)
                        .Take(4)
                        .Select(b => new ActivityDto
                        {
                            ActionName = "Proposal Submitted",
                            ProjectTitle = b.Project.Title,
                            Date = b.CreatedAt,
                            Status = b.Status.ToString(),
                            IconClass = "M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z",
                            ColorClass = "indigo"
                        })
                        .ToListAsync();

                    var freelancerMessages = await _unitOfWork.Repository<Message>()
                        .Query()
                        .Include(m => m.Sender)
                        .Include(m => m.Receiver)
                        .Where(m => (m.SenderId == userId || m.ReceiverId == userId) && !m.IsDeleted && m.Subject != "SYSTEM_CONVERSATION_CLOSED")
                        .OrderByDescending(m => m.CreatedAt)
                        .Take(4)
                        .ToListAsync();

                    recentActivities.AddRange(freelancerMessages.Select(m => new ActivityDto
                    {
                        ActionName = m.SenderId == userId ? "Message Sent" : "Message Received",
                        ProjectTitle = m.SenderId == userId
                            ? $"To {m.Receiver.FirstName} {m.Receiver.LastName}"
                            : $"From {m.Sender.FirstName} {m.Sender.LastName}",
                        Date = m.CreatedAt,
                        Status = m.IsRead ? "Read" : "Unread",
                        IconClass = "M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z",
                        ColorClass = "emerald"
                    }));
                }

                ViewBag.TotalProjects = totalProjects;
                ViewBag.ActiveProjects = activeProjects;
                ViewBag.CompletedProjects = completedProjects;
                ViewBag.UnreadMessages = unreadMessages;
                ViewBag.IsFreelancer = isFreelancer;
                ViewBag.IsClient = isClient;
                ViewBag.UserName = User.Identity.Name;
                ViewBag.RecentProjects = recentProjects;
                ViewBag.RecentActivities = recentActivities.OrderByDescending(a => a.Date).Take(4).ToList();

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
