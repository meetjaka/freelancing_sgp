import pandas as pd
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.metrics.pairwise import cosine_similarity

def get_recommendations(req):
    if not req.available_projects:
        return []

    # 1. Create a corpus representing the user's profile
    freelancer_features = " ".join(req.freelancer_skills) + " " + req.freelancer_bio
    
    # If freelancer has no features, return the first few
    if not freelancer_features.strip():
        return [{"project_id": p.id, "score": 0.0} for p in req.available_projects[:req.top_n]]

    # 2. Extract features from each project
    project_features = []
    project_ids = []
    for proj in req.available_projects:
        features = f"{proj.title} {proj.description} {' '.join(proj.skills)} {proj.category}"
        project_features.append(features)
        project_ids.append(proj.id)
    
    # 3. Use TF-IDF and Cosine Similarity
    corpus = [freelancer_features] + project_features
    vectorizer = TfidfVectorizer(stop_words='english')
    
    try:
        tfidf_matrix = vectorizer.fit_transform(corpus)
    except ValueError:
        # Happens if vocab is empty (e.g. only stop words)
        return [{"project_id": p.id, "score": 0.0} for p in req.available_projects[:req.top_n]]
    
    # 4. First row is freelancer, the rest are projects
    freelancer_vec = tfidf_matrix[0]
    projects_vec = tfidf_matrix[1:]
    
    # Compute similarity array
    similarities = cosine_similarity(freelancer_vec, projects_vec).flatten()
    
    # 5. Build results, sorting by highest similarity
    results = []
    for idx, pid in enumerate(project_ids):
        results.append({
            "project_id": pid,
            "score": round(float(similarities[idx]), 4)
        })
        
    results.sort(key=lambda x: x["score"], reverse=True)
    return results[:req.top_n]
