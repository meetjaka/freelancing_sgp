using Microsoft.AspNetCore.Mvc;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Services.Interfaces;

namespace SGP_Freelancing.Controllers.Api
{
    [Route("api/ml")]
    [ApiController]
    public class MLApiController : ControllerBase
    {
        private readonly IMLService _mlService;
        private readonly ILogger<MLApiController> _logger;

        public MLApiController(IMLService mlService, ILogger<MLApiController> logger)
        {
            _mlService = mlService;
            _logger = logger;
        }

        [HttpPost("predict-budget")]
        public async Task<IActionResult> PredictBudget([FromBody] BudgetPredictRequest request)
        {
            var result = await _mlService.PredictBudgetAsync(request);
            if (result == null) return BadRequest("Failed to predict budget");
            return Ok(result);
        }

        [HttpPost("predict-category")]
        public async Task<IActionResult> PredictCategory([FromBody] CategoryPredictRequest request)
        {
            var result = await _mlService.PredictCategoryAsync(request);
            if (result == null) return BadRequest("Failed to predict category");
            return Ok(result);
        }

        [HttpPost("check-spam")]
        public async Task<IActionResult> CheckSpam([FromBody] SpamCheckRequest request)
        {
            var result = await _mlService.CheckSpamAsync(request);
            if (result == null) return BadRequest("Failed to check spam");
            return Ok(result);
        }

        [HttpPost("extract-skills")]
        public async Task<IActionResult> ExtractSkills([FromBody] SkillExtractRequest request)
        {
            var result = await _mlService.ExtractSkillsAsync(request);
            if (result == null) return BadRequest("Failed to extract skills");
            return Ok(result);
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] SemanticSearchRequest request)
        {
            var result = await _mlService.SearchAsync(request);
            if (result == null) return BadRequest("Failed to perform semantic search");
            return Ok(result);
        }

        [HttpPost("recommend")]
        public async Task<IActionResult> Recommend([FromBody] JobRecommendRequest request)
        {
            var result = await _mlService.RecommendAsync(request);
            if (result == null) return BadRequest("Failed to get recommendations");
            return Ok(result);
        }
    }
}
