"""
SGP Freelancing ML Models - FastAPI Server
Serves all 6 ML models via REST API endpoints.

Usage:
    cd ML_Models
    python -m api.main
"""

import sys
import os
import time

# Add parent directory to path
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware

from api.schemas import (
    RecommendRequest, RecommendResponse,
    BudgetPredictRequest, BudgetPredictResponse,
    CategoryPredictRequest, CategoryPredictResponse,
    SearchRequest, SearchResponse,
    SpamCheckRequest, SpamCheckResponse,
    SkillExtractRequest, SkillExtractResponse,
    HealthResponse,
)
from training.data_preprocessor import get_saved_models_path
from models.job_recommender import JobRecommender
from models.budget_predictor import BudgetPredictor
from models.category_tagger import CategoryTagger
from models.semantic_search import SemanticSearch
from models.spam_detector import SpamDetector
from models.skill_extractor import SkillExtractor


# ============================================================
# Initialize FastAPI App
# ============================================================

app = FastAPI(
    title="SGP Freelancing ML Models API",
    description="Machine Learning API for Job Recommendations, Budget Prediction, "
                "Category Tagging, Semantic Search, Spam Detection, and Skill Extraction.",
    version="1.0.0",
)

# Enable CORS for the .NET frontend
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


# ============================================================
# Load Models on Startup
# ============================================================

# Global model instances
recommender = JobRecommender()
budget_predictor = BudgetPredictor()
category_tagger = CategoryTagger()
semantic_search = SemanticSearch()
spam_detector = SpamDetector()
skill_extractor = SkillExtractor()

models_status = {
    "recommender": False,
    "budget_predictor": False,
    "category_tagger": False,
    "semantic_search": False,
    "spam_detector": False,
    "skill_extractor": False,
}


@app.on_event("startup")
async def load_models():
    """Load all trained models on server startup."""
    save_dir = get_saved_models_path()
    
    if not os.path.exists(save_dir):
        print("WARNING: No saved models found. Please run training first:")
        print("  cd ML_Models")
        print("  python -m training.train_all")
        return
    
    print("Loading ML models...")
    start = time.time()
    
    try:
        recommender.load(save_dir)
        models_status["recommender"] = True
    except Exception as e:
        print(f"  Failed to load recommender: {e}")
    
    try:
        budget_predictor.load(save_dir)
        models_status["budget_predictor"] = True
    except Exception as e:
        print(f"  Failed to load budget_predictor: {e}")
    
    try:
        category_tagger.load(save_dir)
        models_status["category_tagger"] = True
    except Exception as e:
        print(f"  Failed to load category_tagger: {e}")
    
    try:
        semantic_search.load(save_dir)
        models_status["semantic_search"] = True
    except Exception as e:
        print(f"  Failed to load semantic_search: {e}")
    
    try:
        spam_detector.load(save_dir)
        models_status["spam_detector"] = True
    except Exception as e:
        print(f"  Failed to load spam_detector: {e}")
    
    try:
        skill_extractor.load(save_dir)
        models_status["skill_extractor"] = True
    except Exception as e:
        print(f"  Failed to load skill_extractor: {e}")
    
    loaded = sum(models_status.values())
    elapsed = time.time() - start
    print(f"Loaded {loaded}/6 models in {elapsed:.1f}s")


# ============================================================
# Endpoints
# ============================================================

@app.get("/", tags=["General"])
def root():
    """Root endpoint."""
    return {"message": "SGP Freelancing ML Models API", "version": "1.0.0"}


@app.get("/health", response_model=HealthResponse, tags=["General"])
def health_check():
    """Check API health and model status."""
    return HealthResponse(
        status="healthy",
        models_loaded=models_status,
        dataset_info={
            "total_models": 6,
            "loaded_models": sum(models_status.values()),
        }
    )


# --- Model 1: Job Recommendations ---

@app.post("/recommend", response_model=RecommendResponse, tags=["Recommendations"])
def recommend_jobs(req: RecommendRequest):
    """
    Get job recommendations for a freelancer based on their skills and bio.
    """
    if not models_status["recommender"]:
        raise HTTPException(status_code=503, detail="Recommender model not loaded. Run training first.")
    
    try:
        results = recommender.recommend(
            freelancer_skills=req.skills,
            freelancer_bio=req.bio,
            top_n=req.top_n
        )
        return RecommendResponse(recommendations=results, count=len(results))
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


# --- Model 2: Budget Prediction ---

@app.post("/predict-budget", response_model=BudgetPredictResponse, tags=["Budget"])
def predict_budget(req: BudgetPredictRequest):
    """
    Predict a fair budget (in USD) for a job posting.
    """
    if not models_status["budget_predictor"]:
        raise HTTPException(status_code=503, detail="Budget predictor not loaded. Run training first.")
    
    try:
        result = budget_predictor.predict(
            title=req.title,
            description=req.description,
            category=req.category,
            experience_level=req.experience_level
        )
        return BudgetPredictResponse(**result)
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


# --- Model 3: Category Tagging ---

@app.post("/predict-category", response_model=CategoryPredictResponse, tags=["Category"])
def predict_category(req: CategoryPredictRequest):
    """
    Auto-predict the category and sub-category for a job from its title and description.
    """
    if not models_status["category_tagger"]:
        raise HTTPException(status_code=503, detail="Category tagger not loaded. Run training first.")
    
    try:
        result = category_tagger.predict(
            title=req.title,
            description=req.description
        )
        return CategoryPredictResponse(**result)
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


# --- Model 4: Semantic Search ---

@app.post("/search", response_model=SearchResponse, tags=["Search"])
def semantic_search_jobs(req: SearchRequest):
    """
    Search for jobs using natural language (semantic search).
    """
    if not models_status["semantic_search"]:
        raise HTTPException(status_code=503, detail="Semantic search not loaded. Run training first.")
    
    try:
        results = semantic_search.search(query=req.query, top_n=req.top_n)
        return SearchResponse(results=results, count=len(results), query=req.query)
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


# --- Model 5: Spam Detection ---

@app.post("/check-spam", response_model=SpamCheckResponse, tags=["Spam Detection"])
def check_spam(req: SpamCheckRequest):
    """
    Check if a job posting is suspicious or potentially fake.
    """
    if not models_status["spam_detector"]:
        raise HTTPException(status_code=503, detail="Spam detector not loaded. Run training first.")
    
    try:
        result = spam_detector.check(
            title=req.title,
            description=req.description,
            budget=req.budget,
            experience_level=req.experience_level,
            category=req.category,
            location=req.location
        )
        return SpamCheckResponse(**result)
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


# --- Model 6: Skill Extraction ---

@app.post("/extract-skills", response_model=SkillExtractResponse, tags=["Skills"])
def extract_skills(req: SkillExtractRequest):
    """
    Extract skills and keywords from a job description.
    """
    if not models_status["skill_extractor"]:
        raise HTTPException(status_code=503, detail="Skill extractor not loaded. Run training first.")
    
    try:
        result = skill_extractor.extract(text=req.text, top_n=req.top_n)
        return SkillExtractResponse(**result)
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


# ============================================================
# Run Server
# ============================================================

if __name__ == "__main__":
    import uvicorn
    print("\nStarting SGP Freelancing ML Models API on http://localhost:8001")
    print("API docs available at http://localhost:8001/docs\n")
    uvicorn.run(app, host="0.0.0.0", port=8001)
