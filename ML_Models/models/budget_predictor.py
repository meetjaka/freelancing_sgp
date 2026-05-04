"""
Model 2: Budget Predictor / Price Estimator
Random Forest Regressor that predicts fair budget for a job posting.

Uses TF-IDF text features from Title+Description, one-hot encoded Category,
and numeric Experience level to predict Budget (in USD).
"""

import os
import joblib
import numpy as np
import pandas as pd
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.ensemble import RandomForestRegressor, GradientBoostingRegressor
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import LabelEncoder
from sklearn.metrics import mean_absolute_error, r2_score
from scipy.sparse import hstack


class BudgetPredictor:
    """Predicts fair budget for a freelancing job posting."""
    
    def __init__(self):
        self.vectorizer = None
        self.category_encoder = None
        self.model = None
        self.is_trained = False
        self.metrics = {}
    
    def train(self, df):
        """
        Train the budget prediction model.
        
        Args:
            df: Preprocessed DataFrame with 'combined_text', 'Budget_USD', etc.
        """
        print("[BudgetPredictor] Training started...")
        
        # Filter out rows with zero or invalid budget
        train_df = df[df["Budget_USD"] > 0].copy()
        
        if len(train_df) < 10:
            print("[BudgetPredictor] Not enough data with valid budgets!")
            return
        
        # Remove extreme outliers (budget > 99th percentile)
        upper_limit = train_df["Budget_USD"].quantile(0.99)
        lower_limit = train_df["Budget_USD"].quantile(0.01)
        train_df = train_df[
            (train_df["Budget_USD"] >= lower_limit) & 
            (train_df["Budget_USD"] <= upper_limit)
        ]
        
        print(f"[BudgetPredictor] Training on {len(train_df)} samples (after filtering)")
        
        # Prepare text features
        text_data = (train_df["Title_clean"].fillna("") + " " + train_df["Description_clean"].fillna("")).str.strip()
        
        self.vectorizer = TfidfVectorizer(
            max_features=5000,
            stop_words="english",
            ngram_range=(1, 2),
            min_df=2,
            max_df=0.95,
        )
        text_features = self.vectorizer.fit_transform(text_data)
        
        # Prepare category features
        self.category_encoder = LabelEncoder()
        category_col = train_df["Category_Name_clean"].fillna("unknown")
        category_encoded = self.category_encoder.fit_transform(category_col)
        
        # Prepare numeric features
        experience_levels = train_df["Experience_Level"].values.reshape(-1, 1)
        
        # Combine all features
        from scipy.sparse import csr_matrix
        numeric_features = csr_matrix(
            np.hstack([category_encoded.reshape(-1, 1), experience_levels])
        )
        X = hstack([text_features, numeric_features])
        y = train_df["Budget_USD"].values
        
        # Apply log transform to budget for better regression
        y_log = np.log1p(y)
        
        # Train/test split
        X_train, X_test, y_train, y_test = train_test_split(
            X, y_log, test_size=0.2, random_state=42
        )
        
        # Train Random Forest Regressor (handles sparse matrices natively)
        self.model = RandomForestRegressor(
            n_estimators=150,
            max_depth=15,
            min_samples_split=5,
            random_state=42,
            n_jobs=-1,
        )
        
        self.model.fit(X_train, y_train)
        
        # Evaluate
        y_pred_log = self.model.predict(X_test)
        y_pred = np.expm1(y_pred_log)
        y_actual = np.expm1(y_test)
        
        mae = mean_absolute_error(y_actual, y_pred)
        r2 = r2_score(y_actual, y_pred)
        
        self.metrics = {
            "mae": round(mae, 2),
            "r2_score": round(r2, 4),
            "train_samples": len(y_train),
            "test_samples": len(y_test),
            "budget_range": f"${lower_limit:.2f} - ${upper_limit:.2f}",
        }
        
        self.is_trained = True
        
        print(f"[BudgetPredictor] Training complete!")
        print(f"[BudgetPredictor] MAE: ${mae:.2f}")
        print(f"[BudgetPredictor] R² Score: {r2:.4f}")
    
    def predict(self, title, description, category="", experience_level=2):
        """
        Predict the budget for a job posting.
        
        Args:
            title: Job title string
            description: Job description text
            category: Category name
            experience_level: 1=Entry, 2=Intermediate, 3=Expert
            
        Returns:
            Dict with predicted budget and confidence info
        """
        if not self.is_trained:
            return {"error": "Model not trained yet. Run training first."}
        
        # Prepare text
        text = f"{title.lower()} {description.lower()}".strip()
        text_features = self.vectorizer.transform([text])
        
        # Prepare category
        try:
            cat_encoded = self.category_encoder.transform([category.lower().strip()])
        except ValueError:
            cat_encoded = np.array([0])  # Unknown category
        
        # Combine features
        from scipy.sparse import csr_matrix
        numeric = csr_matrix(np.array([[cat_encoded[0], experience_level]]))
        X = hstack([text_features, numeric])
        
        # Predict (inverse log transform)
        y_pred_log = self.model.predict(X)
        predicted_budget = float(np.expm1(y_pred_log[0]))
        
        # Ensure non-negative
        predicted_budget = max(0, predicted_budget)
        
        return {
            "predicted_budget_usd": round(predicted_budget, 2),
            "confidence": "medium",  # Could be improved with prediction intervals
            "model_metrics": self.metrics,
        }
    
    def save(self, save_dir):
        """Save the trained model to disk."""
        os.makedirs(save_dir, exist_ok=True)
        joblib.dump(self.model, os.path.join(save_dir, "budget_model.pkl"))
        joblib.dump(self.vectorizer, os.path.join(save_dir, "budget_vectorizer.pkl"))
        joblib.dump(self.category_encoder, os.path.join(save_dir, "budget_encoder.pkl"))
        joblib.dump(self.metrics, os.path.join(save_dir, "budget_metrics.pkl"))
        print(f"[BudgetPredictor] Model saved to {save_dir}")
    
    def load(self, save_dir):
        """Load a trained model from disk."""
        self.model = joblib.load(os.path.join(save_dir, "budget_model.pkl"))
        self.vectorizer = joblib.load(os.path.join(save_dir, "budget_vectorizer.pkl"))
        self.category_encoder = joblib.load(os.path.join(save_dir, "budget_encoder.pkl"))
        self.metrics = joblib.load(os.path.join(save_dir, "budget_metrics.pkl"))
        self.is_trained = True
        print(f"[BudgetPredictor] Model loaded from {save_dir}")
