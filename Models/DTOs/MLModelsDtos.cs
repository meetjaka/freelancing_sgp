using System.Text.Json.Serialization;

namespace SGP_Freelancing.Models.DTOs
{
    // ----- Requests -----

    public class BudgetPredictRequest
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("experience_level")]
        public int ExperienceLevel { get; set; } = 2;
    }

    public class CategoryPredictRequest
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }

    public class SpamCheckRequest
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("budget")]
        public decimal Budget { get; set; }

        [JsonPropertyName("experience_level")]
        public int ExperienceLevel { get; set; } = 2;

        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("location")]
        public string Location { get; set; } = string.Empty;
    }

    public class SkillExtractRequest
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("top_n")]
        public int TopN { get; set; } = 15;
    }

    public class SemanticSearchRequest
    {
        [JsonPropertyName("query")]
        public string Query { get; set; } = string.Empty;

        [JsonPropertyName("top_n")]
        public int TopN { get; set; } = 10;
        
        [JsonPropertyName("threshold")]
        public double Threshold { get; set; } = 0.1;
    }

    public class JobRecommendRequest
    {
        [JsonPropertyName("skills")]
        public List<string> Skills { get; set; } = new();

        [JsonPropertyName("top_n")]
        public int TopN { get; set; } = 10;
    }

    // ----- Responses -----

    public class BudgetPredictResponse
    {
        [JsonPropertyName("predicted_budget_usd")]
        public decimal PredictedBudgetUsd { get; set; }

        [JsonPropertyName("confidence")]
        public string Confidence { get; set; } = string.Empty;
    }

    public class CategoryInfo
    {
        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }
    }

    public class CategoryPredictResponse
    {
        [JsonPropertyName("predicted_category")]
        public string PredictedCategory { get; set; } = string.Empty;

        [JsonPropertyName("category_confidence")]
        public double CategoryConfidence { get; set; }

        [JsonPropertyName("top_3_categories")]
        public List<CategoryInfo> Top3Categories { get; set; } = new();
    }

    public class SpamCheckResponse
    {
        [JsonPropertyName("is_spam")]
        public bool IsSpam { get; set; }

        [JsonPropertyName("risk_score")]
        public int RiskScore { get; set; }

        [JsonPropertyName("risk_level")]
        public string RiskLevel { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("red_flags")]
        public List<string> RedFlags { get; set; } = new();
    }

    public class ExtractedSkill
    {
        [JsonPropertyName("skill")]
        public string Skill { get; set; } = string.Empty;

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; } = string.Empty;
    }

    public class SkillExtractResponse
    {
        [JsonPropertyName("skills")]
        public List<ExtractedSkill> Skills { get; set; } = new();

        [JsonPropertyName("total_found")]
        public int TotalFound { get; set; }
    }

    public class SearchResultItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("budget")]
        public decimal Budget { get; set; }

        [JsonPropertyName("experience_level")]
        public int ExperienceLevel { get; set; }

        [JsonPropertyName("description_snippet")]
        public string DescriptionSnippet { get; set; } = string.Empty;

        [JsonPropertyName("similarity_score")]
        public double SimilarityScore { get; set; }

        [JsonPropertyName("relevance_score")]
        public double RelevanceScore { get; set; } // Alias to SimilarityScore used by different endpoints
    }

    public class SemanticSearchResponse
    {
        [JsonPropertyName("query")]
        public string Query { get; set; } = string.Empty;

        [JsonPropertyName("count")]
        public int TotalFound { get; set; }

        [JsonPropertyName("results")]
        public List<SearchResultItem> Results { get; set; } = new();
    }

    public class JobRecommendResponse
    {
        [JsonPropertyName("user_skills")]
        public List<string> UserSkills { get; set; } = new();

        [JsonPropertyName("count")]
        public int TotalFound { get; set; }

        [JsonPropertyName("recommendations")]
        public List<SearchResultItem> Recommendations { get; set; } = new();
    }
}
