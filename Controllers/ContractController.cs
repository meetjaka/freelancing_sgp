using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGP_Freelancing.Models.ViewModels;
using SGP_Freelancing.Services.Interfaces;
using System.Security.Claims;

namespace SGP_Freelancing.Controllers
{
    [Authorize]
    public class ContractController : Controller
    {
        private readonly IContractService _contractService;
        private readonly ILogger<ContractController> _logger;

        public ContractController(IContractService contractService, ILogger<ContractController> logger)
        {
            _contractService = contractService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var isClient = User.IsInRole("Client");
                
                var contracts = isClient 
                    ? await _contractService.GetContractsByClientAsync(userId)
                    : await _contractService.GetContractsByFreelancerAsync(userId);
                
                return View(contracts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading contracts");
                return View(new List<SGP_Freelancing.Models.DTOs.ContractDto>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var viewModel = await _contractService.GetContractDetailsAsync(id, userId);
                
                if (viewModel == null)
                    return NotFound();
                
                // Set CanReview flag logic here if not set by service
                // For now we assume the service or mapper handles it, 
                // but let's be safe and check if it's completed.
                if (viewModel.Contract.Status == "Completed")
                {
                    viewModel.CanReview = true;
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading contract details {id}");
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await _contractService.CompleteContractAsync(id, userId);
                
                if (result.Success)
                    TempData["Success"] = result.Message;
                else
                    TempData["Error"] = result.Message;
                
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error completing contract {id}");
                TempData["Error"] = "An error occurred";
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}
