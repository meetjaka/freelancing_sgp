"""
Model 5: Spam / Fake Job Detection
Isolation Forest based anomaly detection to flag suspicious job postings.

Analyzes statistical features of job postings (text length, budget patterns,
missing fields, experience mismatches) to detect outliers.
"""

import os
import joblib
import numpy as np
import pandas as pd
from sklearn.ensemble import IsolationForest
from sklearn.preprocessing import StandardScaler


class SpamDetector:
    """Detects suspicious/fake job postings using anomaly detection."""
    
    def __init__(self):
        self.model = None
        self.scaler = None
        self.is_trained = False
        self.feature_names = []
        self.metrics = {}
    
    def _extract_features(self, df=None, single_row=None):
        """
        Extract statistical features for spam detection.
        Works with both a DataFrame (training) and single dict (inference).
        """
        if df is not None:
            features = pd.DataFrame()
            
            # 1. Description length (character count)
            features["desc_length"] = df["Description_clean"].fillna("").str.len()
            
            # 2. Description word count
            features["desc_word_count"] = df["Description_clean"].fillna("").str.split().str.len().fillna(0)
            
            # 3. Title length
            features["title_length"] = df["Title_clean"].fillna("").str.len()
            
            # 4. Title word count
            features["title_word_count"] = df["Title_clean"].fillna("").str.split().str.len().fillna(0)
            
            # 5. Budget in USD
            features["budget_usd"] = df["Budget_USD"].fillna(0)
            
            # 6. Budget to description length ratio (suspiciously high budget for short desc)
            features["budget_per_word"] = np.where(
                features["desc_word_count"] > 0,
                features["budget_usd"] / features["desc_word_count"],
                0
            )
            
            # 7. Experience level
            features["experience_level"] = df["Experience_Level"].fillna(2)
            
            # 8. Budget vs Experience mismatch (high budget + low experience = suspicious)
            features["budget_exp_ratio"] = np.where(
                features["experience_level"] > 0,
                features["budget_usd"] / features["experience_level"],
                features["budget_usd"]
            )
            
            # 9. Missing fields count
            check_cols = ["Title", "Description", "Category_Name", "Budget", "Experience"]
            features["missing_fields"] = 0
            for col in check_cols:
                if col in df.columns:
                    features["missing_fields"] += df[col].isna().astype(int)
            
            # 10. Has location info
            features["has_location"] = 0
            if "Location" in df.columns:
                features["has_location"] = df["Location"].notna().astype(int)
            
            # 11. Description has contact info (emails, phone numbers - suspicious)
            features["has_contact_info"] = 0
            if "Description_clean" in df.columns:
                desc = df["Description_clean"].fillna("")
                features["has_contact_info"] = (
                    desc.str.contains(r"\b[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,}\b", regex=True, na=False) |
                    desc.str.contains(r"\b\d{10,}\b", regex=True, na=False)
                ).astype(int)
            
            # 12. Excessive caps ratio in original title (shouting = suspicious)
            if "Title" in df.columns:
                original_title = df["Title"].fillna("")
                alpha_chars = original_title.str.replace(r"[^a-zA-Z]", "", regex=True)
                upper_chars = original_title.str.replace(r"[^A-Z]", "", regex=True)
                features["caps_ratio"] = np.where(
                    alpha_chars.str.len() > 0,
                    upper_chars.str.len() / alpha_chars.str.len(),
                    0
                )
            else:
                features["caps_ratio"] = 0
            
            self.feature_names = features.columns.tolist()
            return features.fillna(0)
        
        elif single_row is not None:
            # Single prediction mode
            desc = single_row.get("description", "").lower()
            title = single_row.get("title", "").lower()
            budget = float(single_row.get("budget", 0))
            experience = int(single_row.get("experience_level", 2))
            
            desc_words = len(desc.split()) if desc else 0
            title_words = len(title.split()) if title else 0
            
            import re
            has_contact = 1 if (
                re.search(r"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,}", desc) or
                re.search(r"\b\d{10,}\b", desc)
            ) else 0
            
            original_title = single_row.get("title", "")
            alpha = "".join(c for c in original_title if c.isalpha())
            upper = "".join(c for c in original_title if c.isupper())
            caps_ratio = len(upper) / len(alpha) if alpha else 0
            
            feature_dict = {
                "desc_length": len(desc),
                "desc_word_count": desc_words,
                "title_length": len(title),
                "title_word_count": title_words,
                "budget_usd": budget,
                "budget_per_word": budget / desc_words if desc_words > 0 else 0,
                "experience_level": experience,
                "budget_exp_ratio": budget / experience if experience > 0 else budget,
                "missing_fields": sum([
                    1 if not title else 0,
                    1 if not desc else 0,
                    1 if not single_row.get("category", "") else 0,
                    1 if budget == 0 else 0,
                    1 if experience == 0 else 0,
                ]),
                "has_location": 1 if single_row.get("location", "") else 0,
                "has_contact_info": has_contact,
                "caps_ratio": caps_ratio,
            }
            
            return pd.DataFrame([feature_dict])[self.feature_names]
    
    def train(self, df):
        """
        Train the spam detector on the preprocessed dataset.
        
        Args:
            df: Preprocessed DataFrame
        """
        print("[SpamDetector] Training started...")
        
        features = self._extract_features(df=df)
        
        if len(features) < 10:
            print("[SpamDetector] Not enough data!")
            return
        
        # Scale features
        self.scaler = StandardScaler()
        X_scaled = self.scaler.fit_transform(features)
        
        # Train Isolation Forest
        self.model = IsolationForest(
            n_estimators=200,
            contamination=0.05,  # Assume ~5% are suspicious/spam
            max_samples="auto",
            random_state=42,
            n_jobs=-1,
        )
        self.model.fit(X_scaled)
        
        # Get stats on training data
        scores = self.model.decision_function(X_scaled)
        predictions = self.model.predict(X_scaled)
        n_anomalies = (predictions == -1).sum()
        
        self.metrics = {
            "training_samples": len(features),
            "anomalies_detected": int(n_anomalies),
            "anomaly_percentage": round(n_anomalies / len(features) * 100, 2),
            "score_threshold": round(float(np.percentile(scores, 5)), 4),
        }
        
        self.is_trained = True
        
        print(f"[SpamDetector] Training complete!")
        print(f"[SpamDetector] Detected {n_anomalies} anomalies ({self.metrics['anomaly_percentage']}%)")
    
    def check(self, title, description="", budget=0, experience_level=2, 
              category="", location=""):
        """
        Check if a job posting is suspicious.
        
        Args:
            title: Job title text
            description: Job description text
            budget: Budget amount (in USD)
            experience_level: 1=Entry, 2=Intermediate, 3=Expert
            category: Category name
            location: Location string
            
        Returns:
            Dict with spam score and risk assessment
        """
        if not self.is_trained:
            return {"error": "Model not trained yet. Run training first."}
        
        single_row = {
            "title": title,
            "description": description,
            "budget": budget,
            "experience_level": experience_level,
            "category": category,
            "location": location,
        }
        
        features = self._extract_features(single_row=single_row)
        X_scaled = self.scaler.transform(features)
        
        # Get anomaly score (negative = more anomalous)
        anomaly_score = float(self.model.decision_function(X_scaled)[0])
        prediction = int(self.model.predict(X_scaled)[0])  # 1=normal, -1=anomaly
        
        # Convert to a 0-100 risk score (higher = more suspicious)
        # decision_function returns negative values for anomalies
        risk_score = max(0, min(100, int((1 - (anomaly_score + 0.5)) * 100)))
        
        # Determine risk level
        if risk_score >= 70:
            risk_level = "HIGH"
            message = "This job posting appears suspicious. Review carefully before publishing."
        elif risk_score >= 40:
            risk_level = "MEDIUM"
            message = "This job posting has some unusual characteristics. Consider reviewing."
        else:
            risk_level = "LOW"
            message = "This job posting appears normal."
        
        # Identify specific red flags
        red_flags = []
        raw_features = features.iloc[0]
        
        if raw_features["desc_word_count"] < 10:
            red_flags.append("Very short description")
        if raw_features["budget_per_word"] > 50:
            red_flags.append("Unusually high budget relative to description length")
        if raw_features["missing_fields"] >= 2:
            red_flags.append(f"{int(raw_features['missing_fields'])} important fields are missing")
        if raw_features["has_contact_info"] == 1:
            red_flags.append("Description contains contact information (email/phone)")
        if raw_features["caps_ratio"] > 0.7:
            red_flags.append("Excessive use of capital letters in title")
        
        return {
            "is_spam": prediction == -1,
            "risk_score": risk_score,
            "risk_level": risk_level,
            "message": message,
            "red_flags": red_flags,
            "anomaly_score": round(anomaly_score, 4),
        }
    
    def save(self, save_dir):
        """Save the trained model to disk."""
        os.makedirs(save_dir, exist_ok=True)
        joblib.dump(self.model, os.path.join(save_dir, "spam_model.pkl"))
        joblib.dump(self.scaler, os.path.join(save_dir, "spam_scaler.pkl"))
        joblib.dump(self.feature_names, os.path.join(save_dir, "spam_features.pkl"))
        joblib.dump(self.metrics, os.path.join(save_dir, "spam_metrics.pkl"))
        print(f"[SpamDetector] Model saved to {save_dir}")
    
    def load(self, save_dir):
        """Load a trained model from disk."""
        self.model = joblib.load(os.path.join(save_dir, "spam_model.pkl"))
        self.scaler = joblib.load(os.path.join(save_dir, "spam_scaler.pkl"))
        self.feature_names = joblib.load(os.path.join(save_dir, "spam_features.pkl"))
        self.metrics = joblib.load(os.path.join(save_dir, "spam_metrics.pkl"))
        self.is_trained = True
        print(f"[SpamDetector] Model loaded from {save_dir}")
