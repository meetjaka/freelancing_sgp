import pandas as pd

def get_market_insights(projects):
    if not projects:
        return {
            "top_skills": {},
            "average_budget": 0.0,
            "total_projects": 0,
            "category_distribution": {}
        }
        
    # Build dataframe
    data = []
    for p in projects:
        data.append({
            "id": p.id,
            "budget": p.budget,
            "category": p.category,
            "skills": p.skills
        })
    df = pd.DataFrame(data)
    
    # 1. Top in-demand skills
    all_skills = []
    for skill_list in df['skills']:
        all_skills.extend(skill_list)
        
    skill_series = pd.Series(all_skills)
    top_skills = skill_series.value_counts().head(5).to_dict()
    
    # 2. Average Budget
    avg_budget = round(float(df['budget'].mean()), 2)
    
    # 3. Category Distribution
    category_counts = df['category'].value_counts().to_dict()
    
    return {
        "top_skills": top_skills,
        "average_budget": avg_budget,
        "total_projects": len(df),
        "category_distribution": category_counts
    }
