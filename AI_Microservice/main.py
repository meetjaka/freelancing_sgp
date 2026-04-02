from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from typing import List, Dict, Any
import recommender
import insights

app = FastAPI(title="SGP Freelancing AI Microservice", description="API for Recommendations and Insights")

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

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
