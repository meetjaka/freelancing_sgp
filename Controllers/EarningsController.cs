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
    public class EarningsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EarningsController> _logger;

        public EarningsController(ApplicationDbContext context, ILogger<EarningsController> logger)
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

                var now = DateTime.UtcNow;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var startOfLastMonth = startOfMonth.AddMonths(-1);

                // Initialize view model
                var viewModel = new EarningsViewModel
                {
                    IsClient = isClient,
                    MonthlyLabels = new List<string>(),
                    MonthlyData = new List<decimal>()
                };

                // Generate labels for the last 6 months
                for (int i = 5; i >= 0; i--)
                {
                    viewModel.MonthlyLabels.Add(now.AddMonths(-i).ToString("MMM"));
                    viewModel.MonthlyData.Add(0); // Initialize with 0
                }

                if (isClient)
                {
                    // As a client, earnings = spending.
                    var userContracts = await _context.Contracts
                        .Where(c => c.ClientId == userId && !c.IsDeleted)
                        .Select(c => c.Id)
                        .ToListAsync();

                    var transactions = await _context.PaymentTransactions
                        .Include(pt => pt.Contract)
                        .ThenInclude(c => c.Freelancer)
                        .Include(pt => pt.Contract.Project)
                        .Where(pt => userContracts.Contains(pt.ContractId) && !pt.IsDeleted)
                        .OrderByDescending(pt => pt.CreatedAt)
                        .ToListAsync();

                    // Calculate totals
                    viewModel.TotalEarnings = transactions.Where(t => t.Status == PaymentStatus.Completed).Sum(t => t.Amount);
                    viewModel.ThisMonthEarnings = transactions.Where(t => t.Status == PaymentStatus.Completed && t.CreatedAt >= startOfMonth).Sum(t => t.Amount);
                    
                    var lastMonthEarnings = transactions.Where(t => t.Status == PaymentStatus.Completed && t.CreatedAt >= startOfLastMonth && t.CreatedAt < startOfMonth).Sum(t => t.Amount);
                    
                    if (lastMonthEarnings > 0)
                    {
                        viewModel.PercentageChange = ((viewModel.ThisMonthEarnings - lastMonthEarnings) / lastMonthEarnings) * 100;
                    }

                    viewModel.PendingEarnings = transactions.Where(t => t.Status == PaymentStatus.Pending || t.Status == PaymentStatus.Processing).Sum(t => t.Amount);

                    viewModel.RecentTransactions = transactions.Take(10).Select(pt => new TransactionDto
                    {
                        Date = pt.CreatedAt,
                        ProjectTitle = pt.Contract.Project.Title,
                        OtherPartyName = pt.Contract.Freelancer.FirstName + " " + pt.Contract.Freelancer.LastName,
                        Amount = pt.Amount,
                        Status = pt.Status.ToString(),
                        Type = pt.Type.ToString(),
                        IsCredit = false // Client is paying
                    }).ToList();

                    // Monthly data calculation
                    var sixMonthsAgo = now.AddMonths(-5);
                    var startOfSixMonthsAgo = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1);
                    
                    var monthlyGroups = transactions
                        .Where(t => t.Status == PaymentStatus.Completed && t.CreatedAt >= startOfSixMonthsAgo)
                        .GroupBy(t => new { t.CreatedAt.Year, t.CreatedAt.Month })
                        .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(t => t.Amount) })
                        .ToList();

                    foreach (var m in monthlyGroups)
                    {
                        var index = viewModel.MonthlyLabels.IndexOf(new DateTime(m.Year, m.Month, 1).ToString("MMM"));
                        if (index >= 0)
                        {
                            viewModel.MonthlyData[index] = m.Total;
                        }
                    }
                }
                else
                {
                    // As a Freelancer
                    var userContracts = await _context.Contracts
                        .Where(c => c.FreelancerId == userId && !c.IsDeleted)
                        .Select(c => c.Id)
                        .ToListAsync();

                    var transactions = await _context.PaymentTransactions
                        .Include(pt => pt.Contract)
                        .ThenInclude(c => c.Client)
                        .Include(pt => pt.Contract.Project)
                        .Where(pt => userContracts.Contains(pt.ContractId) && !pt.IsDeleted)
                        .OrderByDescending(pt => pt.CreatedAt)
                        .ToListAsync();

                    viewModel.TotalEarnings = transactions.Where(t => t.Status == PaymentStatus.Completed && t.Type != PaymentType.Refund).Sum(t => t.Amount);
                    viewModel.ThisMonthEarnings = transactions.Where(t => t.Status == PaymentStatus.Completed && t.Type != PaymentType.Refund && t.CreatedAt >= startOfMonth).Sum(t => t.Amount);
                    
                    var lastMonthEarnings = transactions.Where(t => t.Status == PaymentStatus.Completed && t.Type != PaymentType.Refund && t.CreatedAt >= startOfLastMonth && t.CreatedAt < startOfMonth).Sum(t => t.Amount);
                    
                    if (lastMonthEarnings > 0)
                    {
                        viewModel.PercentageChange = ((viewModel.ThisMonthEarnings - lastMonthEarnings) / lastMonthEarnings) * 100;
                    }

                    viewModel.PendingEarnings = transactions.Where(t => t.Status == PaymentStatus.Pending || t.Status == PaymentStatus.Processing).Sum(t => t.Amount);

                    viewModel.RecentTransactions = transactions.Take(10).Select(pt => new TransactionDto
                    {
                        Date = pt.CreatedAt,
                        ProjectTitle = pt.Contract.Project.Title,
                        OtherPartyName = pt.Contract.Client.FirstName + " " + pt.Contract.Client.LastName,
                        Amount = pt.Amount,
                        Status = pt.Status.ToString(),
                        Type = pt.Type.ToString(),
                        IsCredit = pt.Type != PaymentType.Refund // Freelancer is receiving
                    }).ToList();

                    // Monthly data calculation
                    var sixMonthsAgo = now.AddMonths(-5);
                    var startOfSixMonthsAgo = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1);
                    
                    var monthlyGroups = transactions
                        .Where(t => t.Status == PaymentStatus.Completed && t.Type != PaymentType.Refund && t.CreatedAt >= startOfSixMonthsAgo)
                        .GroupBy(t => new { t.CreatedAt.Year, t.CreatedAt.Month })
                        .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(t => t.Amount) })
                        .ToList();

                    foreach (var m in monthlyGroups)
                    {
                        var index = viewModel.MonthlyLabels.IndexOf(new DateTime(m.Year, m.Month, 1).ToString("MMM"));
                        if (index >= 0)
                        {
                            viewModel.MonthlyData[index] = m.Total;
                        }
                    }
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading earnings dashboard");
                return View(new EarningsViewModel());
            }
        }
    }
}
