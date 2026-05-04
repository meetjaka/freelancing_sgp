from fastapi import FastAPI, HTTPException
from pydantic import BaseModel, Field
from typing import List, Dict, Any, Optional
import recommender
import insights
import random

app = FastAPI(title="SGP Freelancing AI Microservice", description="API for Recommendations, Predictions and Insights")

# ----- Request Models -----

class ProjectInput(BaseModel):
    id: int
    title: str
    description: str
    skills: List[str]
    category: str
    budget: float

class RecommenderRequest(BaseModel):
    freelancer_skills: List[str]
    freelancer_bio: str = ""
    available_projects: List[ProjectInput]
    top_n: int = 10

class InsightsRequest(BaseModel):
    projects: List[ProjectInput]

class BudgetPredictRequest(BaseModel):
    title: str
    description: str
    category: str
    experience_level: int = 2

class CategoryPredictRequest(BaseModel):
    title: str
    description: str

class SpamCheckRequest(BaseModel):
    title: str
    description: str
    budget: float
    experience_level: int = 2
    category: str = ""
    location: str = ""

class SkillExtractRequest(BaseModel):
    text: str
    top_n: int = 15

class SemanticSearchRequest(BaseModel):
    query: str
    top_n: int = 10
    threshold: float = 0.1

class JobRecommendRequest(BaseModel):
    skills: List[str] = []
    top_n: int = 10

# ----- Endpoints -----

@app.get("/")
def read_root():
    return {"status": "AI Microservice is running!"}

@app.post("/recommend")
def recommend_projects(req: RecommenderRequest):
    try:
        recommended = recommender.get_recommendations(req)
        return {"recommendations": recommended}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/insights")
def generate_insights(req: InsightsRequest):
    try:
        data = insights.get_market_insights(req.projects)
        return {"insights": data}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/predict-category")
def predict_category(req: CategoryPredictRequest):
    # Logic to predict category based on title and description
    # Aligned with the frontend dropdown options
    categories = [
        "Web Development", "Mobile Development", "Design & Creative", 
        "Writing & Translation", "Data Science & AI", "Digital Marketing"
    ]
    
    # Simple keyword-based prediction
    text = (req.title + " " + req.description).lower()
    predicted = "Web Development" # Default
    
    if any(word in text for word in ["app", "mobile", "ios", "android", "flutter", "swift", "kotlin"]):
        predicted = "Mobile Development"
    elif any(word in text for word in ["logo", "design", "graphic", "illustrator", "photoshop", "creative", "ui", "ux", "figma"]):
        predicted = "Design & Creative"
    elif any(word in text for word in ["write", "article", "blog", "content", "copywriting", "translation", "translate"]):
        predicted = "Writing & Translation"
    elif any(word in text for word in ["data", "ml", "ai", "analysis", "python", "analytics", "science"]):
        predicted = "Data Science & AI"
    elif any(word in text for word in ["marketing", "seo", "ads", "social media", "sales"]):
        predicted = "Digital Marketing"
    
    return {
        "predicted_category": predicted,
        "category_confidence": 0.85,
        "top_3_categories": [
            {"category": predicted, "confidence": 0.85},
            {"category": "Other", "confidence": 0.1},
            {"category": "General", "confidence": 0.05}
        ]
    }

@app.post("/extract-skills")
def extract_skills(req: SkillExtractRequest):
    # Common skills to look for
    common_skills = [
        "React", "Angular", "Vue", "Node.js", "Python", "C#", "ASP.NET Core", 
        "Java", "PHP", "Laravel", "Swift", "Kotlin", "Flutter", "React Native",
        "SQL", "MongoDB", "PostgreSQL", "Docker", "AWS", "Azure", "Git",
        "Figma", "Adobe XD", "Photoshop", "Illustrator", "SEO", "Marketing"
    ]
    
    text = req.text.lower()
    found = []
    for skill in common_skills:
        if skill.lower() in text:
            found.append({"skill": skill, "count": 1, "source": "text_analysis"})
    
    return {
        "skills": found[:req.top_n],
        "total_found": len(found)
    }

@app.post("/predict-budget")
def predict_budget(req: BudgetPredictRequest):
    # Simple budget prediction logic
    base_budget = 500
    if req.experience_level == 1: base_budget = 200
    if req.experience_level == 3: base_budget = 1500
    
    # Scale based on description length as a proxy for complexity
    complexity_multiplier = min(2.0, max(0.5, len(req.description) / 500))
    predicted = base_budget * complexity_multiplier
    
    return {
        "predicted_budget_usd": round(predicted, 2),
        "confidence": "Medium"
    }

@app.post("/check-spam")
def check_spam(req: SpamCheckRequest):
    text = (req.title + " " + req.description).lower()
    spam_keywords = ["money", "crypto", "win", "free", "gift card", "whatsapp", "telegram"]
    
    red_flags = []
    for word in spam_keywords:
        if word in text:
            red_flags.append(f"Contains suspicious keyword: {word}")
            
    is_spam = len(red_flags) > 0
    risk_score = min(100, len(red_flags) * 30)
    
    return {
        "is_spam": is_spam,
        "risk_score": risk_score,
        "risk_level": "High" if is_spam else "Low",
        "message": "Potential spam detected" if is_spam else "Content looks safe",
        "red_flags": red_flags
    }

@app.post("/search")
def semantic_search(req: SemanticSearchRequest):
    # Placeholder for semantic search results
    # In a real app, this would use vector embeddings
    return {
        "query": req.query,
        "count": 0,
        "results": []
    }

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
