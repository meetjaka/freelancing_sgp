using AutoMapper;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Models.Entities;
using SGP_Freelancing.Repositories;
using SGP_Freelancing.Services.Interfaces;
using SGP_Freelancing.Utilities;

namespace SGP_Freelancing.Services
{
    public class BidService : IBidService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<BidService> _logger;

        public BidService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<BidService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<BidDto>> CreateBidAsync(CreateBidDto dto, string freelancerId)
        {
            try
            {
                var project = await _unitOfWork.Projects.GetByIdAsync(dto.ProjectId);
                if (project == null)
                    return ApiResponse<BidDto>.ErrorResponse(Constants.ErrorMessages.ProjectNotFound);

                if (project.Status != ProjectStatus.Open)
                    return ApiResponse<BidDto>.ErrorResponse("Project is not accepting bids");

                // Check if freelancer already bid
                var existingBid = await _unitOfWork.Bids.FirstOrDefaultAsync(
                    b => b.ProjectId == dto.ProjectId && b.FreelancerId == freelancerId);

                if (existingBid != null)
                    return ApiResponse<BidDto>.ErrorResponse("You have already submitted a bid for this project");

                var bid = _mapper.Map<Bid>(dto);
                bid.FreelancerId = freelancerId;
                bid.Status = BidStatus.Pending;

                await _unitOfWork.Bids.AddAsync(bid);
                await _unitOfWork.SaveChangesAsync();

                var bidDto = _mapper.Map<BidDto>(bid);
                return ApiResponse<BidDto>.SuccessResponse(bidDto, Constants.Messages.BidSubmitted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bid");
                return ApiResponse<BidDto>.ErrorResponse("Failed to submit bid");
            }
        }

        public async Task<ApiResponse<BidDto>> AcceptBidAsync(int bidId, string clientId)
        {
            try
            {
                var bid = await _unitOfWork.Bids.GetBidWithDetailsAsync(bidId);
                if (bid == null)
                    return ApiResponse<BidDto>.ErrorResponse(Constants.ErrorMessages.BidNotFound);

                if (bid.Project.ClientId != clientId)
                    return ApiResponse<BidDto>.ErrorResponse(Constants.ErrorMessages.Unauthorized);

                bid.Status = BidStatus.Accepted;
                bid.UpdatedAt = DateTime.UtcNow;

                // Update project status
                var project = bid.Project;
                project.Status = ProjectStatus.InProgress;
                project.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Bids.Update(bid);
                _unitOfWork.Projects.Update(project);
                await _unitOfWork.SaveChangesAsync();

                var bidDto = _mapper.Map<BidDto>(bid);
                return ApiResponse<BidDto>.SuccessResponse(bidDto, "Bid accepted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting bid");
                return ApiResponse<BidDto>.ErrorResponse("Failed to accept bid");
            }
        }

        public async Task<ApiResponse<BidDto>> RejectBidAsync(int bidId, string clientId)
        {
            try
            {
                var bid = await _unitOfWork.Bids.GetBidWithDetailsAsync(bidId);
                if (bid == null)
                    return ApiResponse<BidDto>.ErrorResponse(Constants.ErrorMessages.BidNotFound);

                if (bid.Project.ClientId != clientId)
                    return ApiResponse<BidDto>.ErrorResponse(Constants.ErrorMessages.Unauthorized);

                bid.Status = BidStatus.Rejected;
                bid.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Bids.Update(bid);
                await _unitOfWork.SaveChangesAsync();

                var bidDto = _mapper.Map<BidDto>(bid);
                return ApiResponse<BidDto>.SuccessResponse(bidDto, "Bid rejected");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting bid");
                return ApiResponse<BidDto>.ErrorResponse("Failed to reject bid");
            }
        }

        public async Task<ApiResponse<bool>> WithdrawBidAsync(int bidId, string freelancerId)
        {
            try
            {
                var bid = await _unitOfWork.Bids.GetByIdAsync(bidId);
                if (bid == null)
                    return ApiResponse<bool>.ErrorResponse(Constants.ErrorMessages.BidNotFound);

                if (bid.FreelancerId != freelancerId)
                    return ApiResponse<bool>.ErrorResponse(Constants.ErrorMessages.Unauthorized);

                bid.Status = BidStatus.Withdrawn;
                bid.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Bids.Update(bid);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Bid withdrawn successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error withdrawing bid");
                return ApiResponse<bool>.ErrorResponse("Failed to withdraw bid");
            }
        }

        public async Task<List<BidDto>> GetBidsByProjectAsync(int projectId)
        {
            var bids = await _unitOfWork.Bids.GetBidsByProjectAsync(projectId);
            return _mapper.Map<List<BidDto>>(bids);
        }

        public async Task<List<BidDto>> GetBidsByFreelancerAsync(string freelancerId)
        {
            var bids = await _unitOfWork.Bids.GetBidsByFreelancerAsync(freelancerId);
            return _mapper.Map<List<BidDto>>(bids);
        }

        public async Task<BidDto?> GetBidByIdAsync(int bidId)
        {
            var bid = await _unitOfWork.Bids.GetBidWithDetailsAsync(bidId);
            return bid != null ? _mapper.Map<BidDto>(bid) : null;
        }
    }
}
