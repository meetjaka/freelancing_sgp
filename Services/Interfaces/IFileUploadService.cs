using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Utilities;

namespace SGP_Freelancing.Services.Interfaces
{
    public interface IFileUploadService
    {
        Task<ApiResponse<FileAttachmentDto>> UploadFileAsync(IFormFile file, string userId, int? projectId = null, int? contractId = null, int? messageId = null);
        Task<ApiResponse<List<FileAttachmentDto>>> UploadMultipleFilesAsync(List<IFormFile> files, string userId, int? projectId = null, int? contractId = null, int? messageId = null);
        Task<ApiResponse<bool>> DeleteFileAsync(int fileId, string userId);
        Task<ApiResponse<FileAttachmentDto>> GetFileAsync(int fileId);
        Task<List<FileAttachmentDto>> GetFilesByProjectAsync(int projectId);
        Task<List<FileAttachmentDto>> GetFilesByContractAsync(int contractId);
        Task<List<FileAttachmentDto>> GetFilesByMessageAsync(int messageId);
    }
}
