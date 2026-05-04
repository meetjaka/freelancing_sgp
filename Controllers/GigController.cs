using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Models.ViewModels;
using SGP_Freelancing.Repositories;
using SGP_Freelancing.Services.Interfaces;
using System.Security.Claims;

namespace SGP_Freelancing.Controllers
{
    public class GigController : Controller
    {
        private readonly IGigService _gigService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GigController> _logger;

        public GigController(IGigService gigService, IUnitOfWork unitOfWork, IMapper mapper, ILogger<GigController> logger)
        {
            _gigService = gigService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        // Public marketplace
        public async Task<IActionResult> Index(string? search, int? categoryId, int page = 1)
        {
            var services = await _gigService.GetAllServicesAsync(search, categoryId, page);
            var categories = _mapper.Map<List<CategoryDto>>(await _unitOfWork.Categories.GetAllAsync());

            var vm = new GigMarketplaceViewModel
            {
                Services = services.Items,
                Categories = categories,
                SearchTerm = search,
                CategoryId = categoryId,
                PageNumber = services.PageNumber,
                TotalCount = services.TotalCount,
                PageSize = services.PageSize
            };
            return View(vm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var service = await _gigService.GetServiceDetailsAsync(id);
            if (service == null) return NotFound();

            var isClient = User.IsInRole("Client");
            var vm = new GigDetailsViewModel
            {
                Service = service,
                CanOrder = isClient
            };
            return View(vm);
        }

        [Authorize(Roles = "Freelancer")]
        public async Task<IActionResult> Create()
        {
            var categories = _mapper.Map<List<CategoryDto>>(await _unitOfWork.Categories.GetAllAsync());
            var skills = _mapper.Map<List<SkillDto>>(await _unitOfWork.Skills.GetAllAsync());
            var vm = new CreateGigViewModel
            {
                Categories = categories,
                Skills = skills
            };
            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Freelancer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateFreelancerServiceDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _gigService.CreateServiceAsync(dto, userId);
            if (result.Success)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(MyGigs));
            }
            TempData["Error"] = result.Message;
            return RedirectToAction(nameof(Create));
        }

        [Authorize(Roles = "Freelancer")]
        public async Task<IActionResult> MyGigs()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var services = await _gigService.GetMyServicesAsync(userId);
            return View(services);
        }

        [Authorize]
        public async Task<IActionResult> MyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isClient = User.IsInRole("Client");
            var orders = isClient
                ? await _gigService.GetClientOrdersAsync(userId)
                : await _gigService.GetFreelancerOrdersAsync(userId);
            ViewBag.IsClient = isClient;
            return View(orders);
        }

        [HttpPost]
        [Authorize(Roles = "Client")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CreateServiceOrderDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _gigService.PlaceOrderAsync(dto, userId);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(MyOrders));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _gigService.UpdateOrderStatusAsync(orderId, status, userId);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(MyOrders));
        }
    }
}
