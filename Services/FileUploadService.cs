using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Models.Entities;
using SGP_Freelancing.Repositories;
using SGP_Freelancing.Services.Interfaces;
using SGP_Freelancing.Utilities;

namespace SGP_Freelancing.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileUploadService> _logger;
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB
        private readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx", ".txt", ".jpg", ".jpeg", ".png", ".gif", ".zip", ".rar" };

        public FileUploadService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IWebHostEnvironment environment,
            ILogger<FileUploadService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _environment = environment;
            _logger = logger;
        }

        public async Task<ApiResponse<FileAttachmentDto>> UploadFileAsync(IFormFile file, string userId, int? projectId = null, int? contractId = null, int? messageId = null)
        {
            try
            {
                // Validate file
                var validationResult = ValidateFile(file);
                if (!validationResult.Success)
                {
                    return ApiResponse<FileAttachmentDto>.ErrorResponse(validationResult.Message);
                }

                // Generate unique filename
                var fileExtension = Path.GetExtension(file.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

                // Determine folder based on context
                var folder = projectId.HasValue ? "projects" :
                            contractId.HasValue ? "contracts" :
                            messageId.HasValue ? "messages" : "general";

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);
                Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Create database record
                var fileAttachment = new FileAttachment
                {
                    FileName = file.FileName,
                    FilePath = $"/uploads/{folder}/{uniqueFileName}",
                    FileType = file.ContentType,
                    FileSize = file.Length,
                    UploadedById = userId,
                    ProjectId = projectId,
                    ContractId = contractId,
                    MessageId = messageId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<FileAttachment>().AddAsync(fileAttachment);
                await _unitOfWork.SaveChangesAsync();

                var savedAttachment = await _unitOfWork.Repository<FileAttachment>()
                    .Query()
                    .Include(f => f.UploadedBy)
                    .FirstOrDefaultAsync(f => f.Id == fileAttachment.Id);

                _logger.LogInformation($"File uploaded: {file.FileName} by user {userId}");

                var fileDto = _mapper.Map<FileAttachmentDto>(savedAttachment ?? fileAttachment);
                if (string.IsNullOrWhiteSpace(fileDto.UploadedByName))
                {
                    fileDto.UploadedByName = "User";
                }
                return ApiResponse<FileAttachmentDto>.SuccessResponse(fileDto, "File uploaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return ApiResponse<FileAttachmentDto>.ErrorResponse("File upload failed");
            }
        }

        public async Task<ApiResponse<List<FileAttachmentDto>>> UploadMultipleFilesAsync(List<IFormFile> files, string userId, int? projectId = null, int? contractId = null, int? messageId = null)
        {
            try
            {
                var uploadedFiles = new List<FileAttachmentDto>();

                foreach (var file in files)
                {
                    var result = await UploadFileAsync(file, userId, projectId, contractId, messageId);
                    if (result.Success && result.Data != null)
                    {
                        uploadedFiles.Add(result.Data);
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to upload file {file.FileName}: {result.Message}");
                    }
                }

                return ApiResponse<List<FileAttachmentDto>>.SuccessResponse(uploadedFiles, $"{uploadedFiles.Count} file(s) uploaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading multiple files");
                return ApiResponse<List<FileAttachmentDto>>.ErrorResponse("Multiple file upload failed");
            }
        }

        public async Task<ApiResponse<bool>> DeleteFileAsync(int fileId, string userId)
        {
            try
            {
                var fileAttachment = await _unitOfWork.Repository<FileAttachment>()
                    .FirstOrDefaultAsync(f => f.Id == fileId && !f.IsDeleted);

                if (fileAttachment == null)
                {
                    return ApiResponse<bool>.ErrorResponse("File not found");
                }

                if (fileAttachment.UploadedById != userId)
                {
                    return ApiResponse<bool>.ErrorResponse("You are not authorized to delete this file");
                }

                var relativePath = fileAttachment.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var physicalPath = Path.Combine(_environment.WebRootPath, relativePath);
                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                }

                fileAttachment.IsDeleted = true;
                fileAttachment.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<FileAttachment>().Update(fileAttachment);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "File deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file");
                return ApiResponse<bool>.ErrorResponse("File deletion failed");
            }
        }

        public async Task<ApiResponse<FileAttachmentDto>> GetFileAsync(int fileId)
        {
            try
            {
                var fileAttachment = await _unitOfWork.Repository<FileAttachment>()
                    .Query()
                    .Include(f => f.UploadedBy)
                    .FirstOrDefaultAsync(f => f.Id == fileId && !f.IsDeleted);

                if (fileAttachment == null)
                {
                    return ApiResponse<FileAttachmentDto>.ErrorResponse("File not found");
                }

                var fileDto = _mapper.Map<FileAttachmentDto>(fileAttachment);
                return ApiResponse<FileAttachmentDto>.SuccessResponse(fileDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file");
                return ApiResponse<FileAttachmentDto>.ErrorResponse("Failed to retrieve file");
            }
        }

        public async Task<List<FileAttachmentDto>> GetFilesByProjectAsync(int projectId)
        {
            try
            {
                var files = await _unitOfWork.Repository<FileAttachment>()
                    .Query()
                    .Include(f => f.UploadedBy)
                    .Where(f => f.ProjectId == projectId && !f.IsDeleted)
                    .OrderByDescending(f => f.CreatedAt)
                    .ToListAsync();

                return _mapper.Map<List<FileAttachmentDto>>(files);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting project files");
                return new List<FileAttachmentDto>();
            }
        }

        public async Task<List<FileAttachmentDto>> GetFilesByContractAsync(int contractId)
        {
            try
            {
                var files = await _unitOfWork.Repository<FileAttachment>()
                    .Query()
                    .Include(f => f.UploadedBy)
                    .Where(f => f.ContractId == contractId && !f.IsDeleted)
                    .OrderByDescending(f => f.CreatedAt)
                    .ToListAsync();

                return _mapper.Map<List<FileAttachmentDto>>(files);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contract files");
                return new List<FileAttachmentDto>();
            }
        }

        public async Task<List<FileAttachmentDto>> GetFilesByMessageAsync(int messageId)
        {
            try
            {
                var files = await _unitOfWork.Repository<FileAttachment>()
                    .Query()
                    .Include(f => f.UploadedBy)
                    .Where(f => f.MessageId == messageId && !f.IsDeleted)
                    .OrderByDescending(f => f.CreatedAt)
                    .ToListAsync();

                return _mapper.Map<List<FileAttachmentDto>>(files);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting message files");
                return new List<FileAttachmentDto>();
            }
        }

        private ApiResponse<bool> ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return ApiResponse<bool>.ErrorResponse("No file provided");
            }

            if (file.Length > MaxFileSize)
            {
                return ApiResponse<bool>.ErrorResponse($"File size must not exceed {MaxFileSize / (1024 * 1024)}MB");
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(fileExtension))
            {
                return ApiResponse<bool>.ErrorResponse($"File type {fileExtension} is not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}");
            }

            return ApiResponse<bool>.SuccessResponse(true);
        }
    }
}
