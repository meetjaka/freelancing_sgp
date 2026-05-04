"""
Pydantic schemas for the ML Models API.
Defines request/response models for all 6 endpoints.
"""

from pydantic import BaseModel, Field
from typing import List, Optional, Dict, Any


# ============================================================
# Model 1: Job Recommendation
# ============================================================

class RecommendRequest(BaseModel):
    skills: List[str] = Field(..., description="List of freelancer skills", example=["python", "django", "react"])
    bio: str = Field("", description="Freelancer bio/description text")
    top_n: int = Field(10, description="Number of recommendations to return", ge=1, le=50)

class RecommendedJob(BaseModel):
    index: int
    title: str
    category: str
    sub_category: str
    budget: float
    experience: str
    location: str
    similarity_score: float

class RecommendResponse(BaseModel):
    recommendations: List[Dict[str, Any]]
    count: int


# ============================================================
# Model 2: Budget Prediction
# ============================================================

class BudgetPredictRequest(BaseModel):
    title: str = Field(..., description="Job title", example="Build a React website")
    description: str = Field("", description="Job description text")
    category: str = Field("", description="Job category name")
    experience_level: int = Field(2, description="1=Entry, 2=Intermediate, 3=Expert", ge=1, le=3)

class BudgetPredictResponse(BaseModel):
    predicted_budget_usd: float
    confidence: str
    model_metrics: Dict[str, Any]


# ============================================================
# Model 3: Category Tagging
# ============================================================

class CategoryPredictRequest(BaseModel):
    title: str = Field(..., description="Job title", example="Design a mobile app UI")
    description: str = Field("", description="Job description text")

class CategoryPredictResponse(BaseModel):
    predicted_category: str
    category_confidence: float
    top_3_categories: List[Dict[str, Any]]
    predicted_subcategory: str
    subcategory_confidence: float


# ============================================================
# Model 4: Semantic Search
# ============================================================

class SearchRequest(BaseModel):
    query: str = Field(..., description="Natural language search query", example="e-commerce website development")
    top_n: int = Field(10, description="Number of results to return", ge=1, le=50)

class SearchResponse(BaseModel):
    results: List[Dict[str, Any]]
    count: int
    query: str


# ============================================================
# Model 5: Spam Detection
# ============================================================

class SpamCheckRequest(BaseModel):
    title: str = Field(..., description="Job title")
    description: str = Field("", description="Job description text")
    budget: float = Field(0, description="Budget in USD")
    experience_level: int = Field(2, description="1=Entry, 2=Intermediate, 3=Expert", ge=1, le=3)
    category: str = Field("", description="Job category")
    location: str = Field("", description="Job location")

class SpamCheckResponse(BaseModel):
    is_spam: bool
    risk_score: int
    risk_level: str
    message: str
    red_flags: List[str]
    anomaly_score: float


# ============================================================
# Model 6: Skill Extraction
# ============================================================

class SkillExtractRequest(BaseModel):
    text: str = Field(..., description="Job description text to extract skills from")
    top_n: int = Field(15, description="Maximum number of skills to return", ge=1, le=50)

class ExtractedSkill(BaseModel):
    skill: str
    count: int
    source: str

class SkillExtractResponse(BaseModel):
    skills: List[Dict[str, Any]]
    keywords: List[Dict[str, Any]]
    total_found: int


# ============================================================
# General
# ============================================================

class HealthResponse(BaseModel):
    status: str
    models_loaded: Dict[str, bool]
    dataset_info: Dict[str, Any]

class ErrorResponse(BaseModel):
    error: str
    detail: str = ""
