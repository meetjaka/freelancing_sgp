"""
Model 1: Job Recommendation Engine
Content-Based Filtering using TF-IDF + Cosine Similarity.

Recommends relevant jobs to freelancers based on their skills and bio,
by matching against job Title, Description, Category, and Sub Category.
"""

import os
import joblib
import numpy as np
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.metrics.pairwise import cosine_similarity


class JobRecommender:
    """Content-based job recommendation engine."""
    
    def __init__(self):
        self.vectorizer = None
        self.tfidf_matrix = None
        self.job_data = None  # List of dicts with job metadata
        self.is_trained = False
    
    def train(self, df):
        """
        Train the recommender on the preprocessed dataset.
        
        Args:
            df: Preprocessed DataFrame with 'combined_text', 'Title', etc.
        """
        print("[JobRecommender] Training started...")
        
        # Prepare job corpus
        texts = df["combined_text"].tolist()
        
        if not texts:
            print("[JobRecommender] No data to train on!")
            return
        
        # Store job metadata for returning with recommendations
        self.job_data = []
        for idx, row in df.iterrows():
            self.job_data.append({
                "index": idx,
                "title": row.get("Title", ""),
                "category": row.get("Category_Name", ""),
                "sub_category": row.get("Sub_Category", ""),
                "budget": float(row.get("Budget_USD", 0)),
                "experience": row.get("Experience", ""),
                "location": row.get("Location", ""),
            })
        
        # Fit TF-IDF vectorizer on job texts
        self.vectorizer = TfidfVectorizer(
            max_features=10000,
            stop_words="english",
            ngram_range=(1, 2),  # Unigrams and bigrams
            min_df=2,
            max_df=0.95,
        )
        
        self.tfidf_matrix = self.vectorizer.fit_transform(texts)
        self.is_trained = True
        
        print(f"[JobRecommender] Trained on {len(texts)} jobs.")
        print(f"[JobRecommender] Vocabulary size: {len(self.vectorizer.vocabulary_)}")
    
    def recommend(self, freelancer_skills, freelancer_bio="", top_n=10):
        """
        Get job recommendations for a freelancer.
        
        Args:
            freelancer_skills: List of skill strings or single string
            freelancer_bio: Freelancer's bio/description text
            top_n: Number of recommendations to return
            
        Returns:
            List of dicts with job info and similarity scores
        """
        if not self.is_trained:
            return {"error": "Model not trained yet. Run training first."}
        
        # Build freelancer profile text
        if isinstance(freelancer_skills, list):
            skills_text = " ".join(freelancer_skills)
        else:
            skills_text = str(freelancer_skills)
        
        profile_text = f"{skills_text} {freelancer_bio}".strip().lower()
        
        if not profile_text:
            return []
        
        # Transform freelancer profile using the trained vectorizer
        profile_vec = self.vectorizer.transform([profile_text])
        
        # Compute cosine similarity between profile and all jobs
        similarities = cosine_similarity(profile_vec, self.tfidf_matrix).flatten()
        
        # Get top N indices (sorted by similarity, descending)
        top_indices = similarities.argsort()[::-1][:top_n]
        
        # Build results
        results = []
        for idx in top_indices:
            if similarities[idx] > 0:  # Only include if there's some similarity
                job = self.job_data[idx].copy()
                job["similarity_score"] = round(float(similarities[idx]), 4)
                results.append(job)
        
        return results
    
    def save(self, save_dir):
        """Save the trained model to disk."""
        os.makedirs(save_dir, exist_ok=True)
        joblib.dump(self.vectorizer, os.path.join(save_dir, "recommender_vectorizer.pkl"))
        joblib.dump(self.tfidf_matrix, os.path.join(save_dir, "recommender_tfidf_matrix.pkl"))
        joblib.dump(self.job_data, os.path.join(save_dir, "recommender_job_data.pkl"))
        print(f"[JobRecommender] Model saved to {save_dir}")
    
    def load(self, save_dir):
        """Load a trained model from disk."""
        self.vectorizer = joblib.load(os.path.join(save_dir, "recommender_vectorizer.pkl"))
        self.tfidf_matrix = joblib.load(os.path.join(save_dir, "recommender_tfidf_matrix.pkl"))
        self.job_data = joblib.load(os.path.join(save_dir, "recommender_job_data.pkl"))
        self.is_trained = True
        print(f"[JobRecommender] Model loaded from {save_dir}")
