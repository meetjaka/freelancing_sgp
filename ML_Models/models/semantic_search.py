"""
Model 4: Smart Search (Semantic Search)
Uses Sentence Transformers to create dense embeddings for jobs,
enabling natural language semantic search.

Falls back to TF-IDF-based search if sentence-transformers is not available.
"""

import os
import joblib
import numpy as np
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.metrics.pairwise import cosine_similarity

# Try to import sentence-transformers (optional dependency)
SENTENCE_TRANSFORMERS_AVAILABLE = False
try:
    from sentence_transformers import SentenceTransformer
    SENTENCE_TRANSFORMERS_AVAILABLE = True
except ImportError:
    pass


class SemanticSearch:
    """Semantic search engine for finding jobs by meaning, not just keywords."""
    
    def __init__(self, use_transformers=True):
        """
        Args:
            use_transformers: If True, uses sentence-transformers for better results.
                            Falls back to TF-IDF if library not available.
        """
        self.use_transformers = use_transformers and SENTENCE_TRANSFORMERS_AVAILABLE
        self.transformer_model = None
        self.tfidf_vectorizer = None
        self.embeddings = None
        self.tfidf_matrix = None
        self.job_data = None
        self.is_trained = False
        
        if use_transformers and not SENTENCE_TRANSFORMERS_AVAILABLE:
            print("[SemanticSearch] sentence-transformers not installed. Using TF-IDF fallback.")
            print("[SemanticSearch] Install with: pip install sentence-transformers")
    
    def train(self, df):
        """
        Build the search index from the preprocessed dataset.
        
        Args:
            df: Preprocessed DataFrame with text and metadata columns.
        """
        print("[SemanticSearch] Building search index...")
        
        # Prepare texts
        texts = (df["Title_clean"].fillna("") + " " + df["Description_clean"].fillna("")).str.strip().tolist()
        
        # Store job metadata
        self.job_data = []
        for idx, row in df.iterrows():
            self.job_data.append({
                "index": idx,
                "title": row.get("Title", ""),
                "description": str(row.get("Description", ""))[:200],  # Truncate for response size
                "category": row.get("Category_Name", ""),
                "sub_category": row.get("Sub_Category", ""),
                "budget": float(row.get("Budget_USD", 0)),
                "experience": row.get("Experience", ""),
                "location": row.get("Location", ""),
            })
        
        if self.use_transformers:
            self._train_transformer(texts)
        else:
            self._train_tfidf(texts)
        
        self.is_trained = True
        print(f"[SemanticSearch] Search index built for {len(texts)} jobs.")
    
    def _train_transformer(self, texts):
        """Build embeddings using sentence-transformers."""
        print("[SemanticSearch] Loading sentence-transformer model (all-MiniLM-L6-v2)...")
        self.transformer_model = SentenceTransformer("all-MiniLM-L6-v2")
        
        print("[SemanticSearch] Encoding job texts (this may take a few minutes)...")
        self.embeddings = self.transformer_model.encode(
            texts,
            show_progress_bar=True,
            batch_size=64,
            normalize_embeddings=True,
        )
        print(f"[SemanticSearch] Generated {self.embeddings.shape[0]} embeddings of dimension {self.embeddings.shape[1]}")
    
    def _train_tfidf(self, texts):
        """Build TF-IDF matrix as fallback search."""
        self.tfidf_vectorizer = TfidfVectorizer(
            max_features=10000,
            stop_words="english",
            ngram_range=(1, 2),
            min_df=2,
            max_df=0.95,
        )
        self.tfidf_matrix = self.tfidf_vectorizer.fit_transform(texts)
        print(f"[SemanticSearch] TF-IDF index built with {self.tfidf_matrix.shape[1]} features")
    
    def search(self, query, top_n=10):
        """
        Search for jobs using natural language query.
        
        Args:
            query: Natural language search query
            top_n: Number of results to return
            
        Returns:
            List of dicts with job info and relevance scores
        """
        if not self.is_trained:
            return {"error": "Search index not built yet. Run training first."}
        
        query = query.strip().lower()
        if not query:
            return []
        
        if self.use_transformers:
            return self._search_transformer(query, top_n)
        else:
            return self._search_tfidf(query, top_n)
    
    def _search_transformer(self, query, top_n):
        """Semantic search using transformer embeddings."""
        query_embedding = self.transformer_model.encode(
            [query], normalize_embeddings=True
        )
        
        # Compute cosine similarity
        similarities = cosine_similarity(query_embedding, self.embeddings).flatten()
        
        # Get top results
        top_indices = similarities.argsort()[::-1][:top_n]
        
        results = []
        for idx in top_indices:
            if similarities[idx] > 0.05:  # Minimum relevance threshold
                job = self.job_data[idx].copy()
                job["relevance_score"] = round(float(similarities[idx]), 4)
                results.append(job)
        
        return results
    
    def _search_tfidf(self, query, top_n):
        """Keyword-enhanced search using TF-IDF."""
        query_vec = self.tfidf_vectorizer.transform([query])
        similarities = cosine_similarity(query_vec, self.tfidf_matrix).flatten()
        
        top_indices = similarities.argsort()[::-1][:top_n]
        
        results = []
        for idx in top_indices:
            if similarities[idx] > 0.01:
                job = self.job_data[idx].copy()
                job["relevance_score"] = round(float(similarities[idx]), 4)
                results.append(job)
        
        return results
    
    def save(self, save_dir):
        """Save the search index to disk."""
        os.makedirs(save_dir, exist_ok=True)
        joblib.dump(self.job_data, os.path.join(save_dir, "search_job_data.pkl"))
        joblib.dump(self.use_transformers, os.path.join(save_dir, "search_mode.pkl"))
        
        if self.use_transformers:
            np.save(os.path.join(save_dir, "job_embeddings.npy"), self.embeddings)
            # Don't save the transformer model itself — it will be reloaded from cache
        else:
            joblib.dump(self.tfidf_vectorizer, os.path.join(save_dir, "search_tfidf_vectorizer.pkl"))
            joblib.dump(self.tfidf_matrix, os.path.join(save_dir, "search_tfidf_matrix.pkl"))
        
        print(f"[SemanticSearch] Search index saved to {save_dir}")
    
    def load(self, save_dir):
        """Load the search index from disk."""
        self.job_data = joblib.load(os.path.join(save_dir, "search_job_data.pkl"))
        self.use_transformers = joblib.load(os.path.join(save_dir, "search_mode.pkl"))
        
        if self.use_transformers:
            self.embeddings = np.load(os.path.join(save_dir, "job_embeddings.npy"))
            if SENTENCE_TRANSFORMERS_AVAILABLE:
                self.transformer_model = SentenceTransformer("all-MiniLM-L6-v2")
            else:
                print("[SemanticSearch] Warning: sentence-transformers not installed. Switching to TF-IDF.")
                # Try loading TF-IDF fallback if available
                tfidf_path = os.path.join(save_dir, "search_tfidf_vectorizer.pkl")
                if os.path.exists(tfidf_path):
                    self.tfidf_vectorizer = joblib.load(tfidf_path)
                    self.tfidf_matrix = joblib.load(os.path.join(save_dir, "search_tfidf_matrix.pkl"))
                    self.use_transformers = False
                else:
                    raise ImportError("sentence-transformers required but not installed")
        else:
            self.tfidf_vectorizer = joblib.load(os.path.join(save_dir, "search_tfidf_vectorizer.pkl"))
            self.tfidf_matrix = joblib.load(os.path.join(save_dir, "search_tfidf_matrix.pkl"))
        
        self.is_trained = True
        print(f"[SemanticSearch] Search index loaded from {save_dir}")
