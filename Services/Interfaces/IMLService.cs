using SGP_Freelancing.Models.DTOs;

namespace SGP_Freelancing.Services.Interfaces
{
    public interface IMLService
    {
        Task<BudgetPredictResponse?> PredictBudgetAsync(BudgetPredictRequest request);
        Task<CategoryPredictResponse?> PredictCategoryAsync(CategoryPredictRequest request);
        Task<SpamCheckResponse?> CheckSpamAsync(SpamCheckRequest request);
        Task<SkillExtractResponse?> ExtractSkillsAsync(SkillExtractRequest request);
        Task<SemanticSearchResponse?> SearchAsync(SemanticSearchRequest request);
        Task<JobRecommendResponse?> RecommendAsync(JobRecommendRequest request);
    }
}
