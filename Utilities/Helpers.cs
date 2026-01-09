namespace SGP_Freelancing.Utilities
{
    /// <summary>
    /// Application-wide constants
    /// </summary>
    public static class Constants
    {
        public static class Roles
        {
            public const string Admin = "Admin";
            public const string Client = "Client";
            public const string Freelancer = "Freelancer";
        }

        public static class Policies
        {
            public const string RequireAdminRole = "RequireAdminRole";
            public const string RequireClientRole = "RequireClientRole";
            public const string RequireFreelancerRole = "RequireFreelancerRole";
            public const string RequireClientOrFreelancer = "RequireClientOrFreelancer";
        }

        public static class Messages
        {
            public const string RegistrationSuccess = "Registration successful! Please check your email to confirm your account.";
            public const string LoginSuccess = "Welcome back!";
            public const string ProfileUpdated = "Profile updated successfully!";
            public const string ProjectCreated = "Project created successfully!";
            public const string BidSubmitted = "Your bid has been submitted successfully!";
            public const string MessageSent = "Message sent successfully!";
            public const string Error = "An error occurred. Please try again.";
        }

        public static class ErrorMessages
        {
            public const string InvalidCredentials = "Invalid email or password.";
            public const string EmailAlreadyExists = "Email already exists.";
            public const string UserNotFound = "User not found.";
            public const string ProjectNotFound = "Project not found.";
            public const string BidNotFound = "Bid not found.";
            public const string Unauthorized = "You are not authorized to perform this action.";
        }
    }

    /// <summary>
    /// API response wrapper
    /// </summary>
    /// <typeparam name="T">Response data type</typeparam>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }

        public static ApiResponse<T> SuccessResponse(T data, string message = "Success")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }
    }

    /// <summary>
    /// Pagination helper
    /// </summary>
    /// <typeparam name="T">List item type</typeparam>
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;
    }
}
