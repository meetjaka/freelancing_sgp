using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SGP_Freelancing.Data;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Models.Entities;
using GigEntity = SGP_Freelancing.Models.Entities.FreelancerService;
using SGP_Freelancing.Services.Interfaces;
using SGP_Freelancing.Utilities;

namespace SGP_Freelancing.Services
{
    public class GigService : IGigService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<GigService> _logger;

        public GigService(ApplicationDbContext context, IMapper mapper, ILogger<GigService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<FreelancerServiceDto>> GetAllServicesAsync(string? search, int? categoryId, int page = 1, int pageSize = 12)
        {
            var query = _context.FreelancerServices
                .Where(s => s.IsActive)
                .Include(s => s.Freelancer).ThenInclude(f => f.FreelancerProfile)
                .Include(s => s.Category)
                .Include(s => s.ServiceSkills).ThenInclude(ss => ss.Skill)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(s => s.Title.Contains(search) || s.Description.Contains(search));
            if (categoryId.HasValue)
                query = query.Where(s => s.CategoryId == categoryId.Value);

            var totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<FreelancerServiceDto>
            {
                Items = _mapper.Map<List<FreelancerServiceDto>>(items),
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<FreelancerServiceDto?> GetServiceDetailsAsync(int id)
        {
            var service = await _context.FreelancerServices
                .Include(s => s.Freelancer).ThenInclude(f => f.FreelancerProfile)
                .Include(s => s.Category)
                .Include(s => s.ServiceSkills).ThenInclude(ss => ss.Skill)
                .FirstOrDefaultAsync(s => s.Id == id);
            return service == null ? null : _mapper.Map<FreelancerServiceDto>(service);
        }

        public async Task<ApiResponse<FreelancerServiceDto>> CreateServiceAsync(CreateFreelancerServiceDto dto, string freelancerId)
        {
            try
            {
                var service = _mapper.Map<GigEntity>(dto);
                service.FreelancerId = freelancerId;
                service.IsActive = true;

                await _context.FreelancerServices.AddAsync(service);
                await _context.SaveChangesAsync();

                if (dto.SkillIds?.Any() == true)
                {
                    foreach (var skillId in dto.SkillIds)
                    {
                        _context.FreelancerServiceSkills.Add(new FreelancerServiceSkill
                        {
                            FreelancerServiceId = service.Id,
                            SkillId = skillId
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                var full = await GetServiceDetailsAsync(service.Id);
                return ApiResponse<FreelancerServiceDto>.SuccessResponse(full!, "Service created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service");
                return ApiResponse<FreelancerServiceDto>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<bool>> UpdateServiceAsync(int id, CreateFreelancerServiceDto dto, string freelancerId)
        {
            var service = await _context.FreelancerServices.FirstOrDefaultAsync(s => s.Id == id);
            if (service == null) return ApiResponse<bool>.ErrorResponse("Service not found");
            if (service.FreelancerId != freelancerId) return ApiResponse<bool>.ErrorResponse("Unauthorized");

            service.Title = dto.Title;
            service.Description = dto.Description;
            service.CategoryId = dto.CategoryId;
            service.Price = dto.Price;
            service.DeliveryDays = dto.DeliveryDays;

            var existingSkills = await _context.FreelancerServiceSkills.Where(ss => ss.FreelancerServiceId == id).ToListAsync();
            _context.FreelancerServiceSkills.RemoveRange(existingSkills);

            if (dto.SkillIds?.Any() == true)
            {
                foreach (var skillId in dto.SkillIds)
                {
                    _context.FreelancerServiceSkills.Add(new FreelancerServiceSkill { FreelancerServiceId = id, SkillId = skillId });
                }
            }

            await _context.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true, "Service updated");
        }

        public async Task<ApiResponse<bool>> DeleteServiceAsync(int id, string freelancerId)
        {
            var service = await _context.FreelancerServices.FirstOrDefaultAsync(s => s.Id == id);
            if (service == null) return ApiResponse<bool>.ErrorResponse("Service not found");
            if (service.FreelancerId != freelancerId) return ApiResponse<bool>.ErrorResponse("Unauthorized");

            service.IsDeleted = true;
            await _context.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true, "Service deleted");
        }

        public async Task<List<FreelancerServiceDto>> GetMyServicesAsync(string freelancerId)
        {
            var services = await _context.FreelancerServices
                .Where(s => s.FreelancerId == freelancerId)
                .Include(s => s.Category)
                .Include(s => s.Freelancer).ThenInclude(f => f.FreelancerProfile)
                .Include(s => s.ServiceSkills).ThenInclude(ss => ss.Skill)
                .OrderByDescending(s => s.CreatedAt).ToListAsync();
            return _mapper.Map<List<FreelancerServiceDto>>(services);
        }

        public async Task<ApiResponse<ServiceOrderDto>> PlaceOrderAsync(CreateServiceOrderDto dto, string clientId)
        {
            try
            {
                var service = await _context.FreelancerServices.FirstOrDefaultAsync(s => s.Id == dto.ServiceId);
                if (service == null) return ApiResponse<ServiceOrderDto>.ErrorResponse("Service not found");

                var order = new ServiceOrder
                {
                    FreelancerServiceId = dto.ServiceId,
                    ClientId = clientId,
                    FreelancerId = service.FreelancerId,
                    Amount = service.Price,
                    Requirements = dto.Requirements,
                    DeliveryDate = DateTime.UtcNow.AddDays(service.DeliveryDays),
                    Status = ServiceOrderStatus.Placed
                };

                await _context.ServiceOrders.AddAsync(order);
                service.TotalOrders++;
                await _context.SaveChangesAsync();

                var orderWithDetails = await _context.ServiceOrders
                    .Include(o => o.FreelancerService)
                    .Include(o => o.Client).Include(o => o.Freelancer)
                    .FirstAsync(o => o.Id == order.Id);

                return ApiResponse<ServiceOrderDto>.SuccessResponse(_mapper.Map<ServiceOrderDto>(orderWithDetails), "Order placed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error placing order");
                return ApiResponse<ServiceOrderDto>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<bool>> UpdateOrderStatusAsync(int orderId, string newStatus, string userId)
        {
            var order = await _context.ServiceOrders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return ApiResponse<bool>.ErrorResponse("Order not found");
            if (order.ClientId != userId && order.FreelancerId != userId) return ApiResponse<bool>.ErrorResponse("Unauthorized");

            if (Enum.TryParse<ServiceOrderStatus>(newStatus, out var status))
            {
                order.Status = status;
                if (status == ServiceOrderStatus.Completed) order.CompletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResponse(true, "Order status updated");
            }
            return ApiResponse<bool>.ErrorResponse("Invalid status");
        }

        public async Task<List<ServiceOrderDto>> GetClientOrdersAsync(string clientId)
        {
            var orders = await _context.ServiceOrders
                .Where(o => o.ClientId == clientId)
                .Include(o => o.FreelancerService).Include(o => o.Client).Include(o => o.Freelancer)
                .OrderByDescending(o => o.CreatedAt).ToListAsync();
            return _mapper.Map<List<ServiceOrderDto>>(orders);
        }

        public async Task<List<ServiceOrderDto>> GetFreelancerOrdersAsync(string freelancerId)
        {
            var orders = await _context.ServiceOrders
                .Where(o => o.FreelancerId == freelancerId)
                .Include(o => o.FreelancerService).Include(o => o.Client).Include(o => o.Freelancer)
                .OrderByDescending(o => o.CreatedAt).ToListAsync();
            return _mapper.Map<List<ServiceOrderDto>>(orders);
        }
    }
}
