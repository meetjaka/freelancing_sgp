using AutoMapper;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Models.Entities;
using SGP_Freelancing.Models.ViewModels;
using SGP_Freelancing.Repositories;
using SGP_Freelancing.Services.Interfaces;
using SGP_Freelancing.Utilities;

namespace SGP_Freelancing.Services
{
    public class ContractService : IContractService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ContractService> _logger;

        public ContractService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ContractService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<ContractDto>> CreateContractAsync(CreateContractDto dto, string clientId)
        {
            try
            {
                // Verify project ownership
                var project = await _unitOfWork.Projects.GetByIdAsync(dto.ProjectId);
                if (project == null || project.ClientId != clientId)
                    return ApiResponse<ContractDto>.ErrorResponse("Project not found or you don't have permission");

                // Check if contract already exists for this project
                var allContracts = await _unitOfWork.Contracts.GetAllAsync();
                var existingContract = allContracts.FirstOrDefault(c => c.ProjectId == dto.ProjectId);
                if (existingContract != null)
                    return ApiResponse<ContractDto>.ErrorResponse("Contract already exists for this project");

                var contract = new Contract
                {
                    ProjectId = dto.ProjectId,
                    FreelancerId = dto.FreelancerId,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    Terms = dto.Terms,
                    Status = ContractStatus.Active
                };

                await _unitOfWork.Contracts.AddAsync(contract);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Contract created: {contract.Id} for project {dto.ProjectId}");

                var contractDto = _mapper.Map<ContractDto>(contract);
                return ApiResponse<ContractDto>.SuccessResponse(contractDto, "Contract created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contract");
                return ApiResponse<ContractDto>.ErrorResponse("An error occurred while creating the contract");
            }
        }

        public async Task<ContractDetailsViewModel?> GetContractDetailsAsync(int id, string userId)
        {
            try
            {
                var contract = await _unitOfWork.Contracts.GetByIdAsync(id);
                if (contract == null)
                    return null;

                // Check permission
                var project = await _unitOfWork.Projects.GetByIdAsync(contract.ProjectId);
                if (project == null || (project.ClientId != userId && contract.FreelancerId != userId))
                    return null;

                var viewModel = _mapper.Map<ContractDetailsViewModel>(contract);
                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting contract details {id}");
                return null;
            }
        }

        public async Task<List<ContractDto>> GetContractsByClientAsync(string clientId)
        {
            try
            {
                var allProjects = await _unitOfWork.Projects.GetAllAsync();
                var clientProjects = allProjects.Where(p => p.ClientId == clientId).ToList();
                
                var allContracts = await _unitOfWork.Contracts.GetAllAsync();
                var contracts = allContracts.Where(c => clientProjects.Any(p => p.Id == c.ProjectId)).ToList();

                return _mapper.Map<List<ContractDto>>(contracts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting contracts for client {clientId}");
                return new List<ContractDto>();
            }
        }

        public async Task<List<ContractDto>> GetContractsByFreelancerAsync(string freelancerId)
        {
            try
            {
                var allContracts = await _unitOfWork.Contracts.GetAllAsync();
                var contracts = allContracts.Where(c => c.FreelancerId == freelancerId).ToList();
                
                return _mapper.Map<List<ContractDto>>(contracts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting contracts for freelancer {freelancerId}");
                return new List<ContractDto>();
            }
        }

        public async Task<List<ContractDto>> GetActiveContractsAsync()
        {
            try
            {
                var allContracts = await _unitOfWork.Contracts.GetAllAsync();
                var activeContracts = allContracts.Where(c => c.Status == ContractStatus.Active).ToList();
                
                return _mapper.Map<List<ContractDto>>(activeContracts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active contracts");
                return new List<ContractDto>();
            }
        }

        public async Task<ApiResponse<bool>> CompleteContractAsync(int id, string userId)
        {
            try
            {
                var contract = await _unitOfWork.Contracts.GetByIdAsync(id);
                if (contract == null)
                    return ApiResponse<bool>.ErrorResponse("Contract not found");

                // Verify permission (client or freelancer)
                var project = await _unitOfWork.Projects.GetByIdAsync(contract.ProjectId);
                if (project == null || (project.ClientId != userId && contract.FreelancerId != userId))
                    return ApiResponse<bool>.ErrorResponse("You don't have permission to complete this contract");

                contract.Status = ContractStatus.Completed;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Contract {id} marked as completed by user {userId}");

                return ApiResponse<bool>.SuccessResponse(true, "Contract marked as completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error completing contract {id}");
                return ApiResponse<bool>.ErrorResponse("An error occurred");
            }
        }

        public async Task<ApiResponse<bool>> CancelContractAsync(int id, string userId)
        {
            try
            {
                var contract = await _unitOfWork.Contracts.GetByIdAsync(id);
                if (contract == null)
                    return ApiResponse<bool>.ErrorResponse("Contract not found");

                // Verify permission
                var project = await _unitOfWork.Projects.GetByIdAsync(contract.ProjectId);
                if (project == null || (project.ClientId != userId && contract.FreelancerId != userId))
                    return ApiResponse<bool>.ErrorResponse("You don't have permission to cancel this contract");

                contract.Status = ContractStatus.Cancelled;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Contract {id} cancelled by user {userId}");

                return ApiResponse<bool>.SuccessResponse(true, "Contract cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error cancelling contract {id}");
                return ApiResponse<bool>.ErrorResponse("An error occurred");
            }
        }
    }
}
