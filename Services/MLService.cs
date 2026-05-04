using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Services.Interfaces;
using System.Text.Json;
using System.Text;

namespace SGP_Freelancing.Services
{
    public class MLService : IMLService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MLService> _logger;
        private readonly string _baseUrl;

        public MLService(HttpClient httpClient, IConfiguration configuration, ILogger<MLService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _baseUrl = _configuration["AiConfig:MLModelsUrl"] ?? "http://localhost:8001";
        }

        private async Task<T?> PostAsync<T>(string endpoint, object payload)
        {
            try
            {
                var jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_baseUrl}/{endpoint}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(responseJson, new JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true 
                    });
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("ML API Error from {Endpoint}: {Status} - {Error}", endpoint, response.StatusCode, error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to ML API at {Endpoint}", endpoint);
            }
            return default;
        }

        public async Task<BudgetPredictResponse?> PredictBudgetAsync(BudgetPredictRequest request)
        {
            return await PostAsync<BudgetPredictResponse>("predict-budget", request);
        }

        public async Task<CategoryPredictResponse?> PredictCategoryAsync(CategoryPredictRequest request)
        {
            return await PostAsync<CategoryPredictResponse>("predict-category", request);
        }

        public async Task<SpamCheckResponse?> CheckSpamAsync(SpamCheckRequest request)
        {
            return await PostAsync<SpamCheckResponse>("check-spam", request);
        }

        public async Task<SkillExtractResponse?> ExtractSkillsAsync(SkillExtractRequest request)
        {
            return await PostAsync<SkillExtractResponse>("extract-skills", request);
        }

        public async Task<SemanticSearchResponse?> SearchAsync(SemanticSearchRequest request)
        {
            return await PostAsync<SemanticSearchResponse>("search", request);
        }

        public async Task<JobRecommendResponse?> RecommendAsync(JobRecommendRequest request)
        {
            return await PostAsync<JobRecommendResponse>("recommend", request);
        }
    }
}
