"""
Training Pipeline - Train All ML Models
Loads the dataset once, preprocesses it, and trains all 6 models sequentially.

Usage:
    cd ML_Models
    python -m training.train_all
"""

import sys
import os
import time

# Add parent directory to path so imports work
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from training.data_preprocessor import get_preprocessed_data, get_saved_models_path
from models.job_recommender import JobRecommender
from models.budget_predictor import BudgetPredictor
from models.category_tagger import CategoryTagger
from models.semantic_search import SemanticSearch
from models.spam_detector import SpamDetector
from models.skill_extractor import SkillExtractor


def train_all_models(csv_filename="Freelance Platform Projects.csv"):
    """
    Train all 6 ML models using the provided dataset.
    
    Args:
        csv_filename: Name of the CSV file in the data/ directory
    """
    total_start = time.time()
    save_dir = get_saved_models_path()
    os.makedirs(save_dir, exist_ok=True)
    
    print("=" * 60)
    print("  SGP Freelancing - ML Models Training Pipeline")
    print("=" * 60)
    
    # Step 1: Load and preprocess data
    print("\n" + "=" * 60)
    print("  STEP 1: Loading & Preprocessing Dataset")
    print("=" * 60)
    
    try:
        df = get_preprocessed_data(csv_filename)
    except FileNotFoundError as e:
        print(f"\nERROR: {e}")
        print("Please place your CSV file in the 'ML_Models/data/' directory.")
        return False
    
    print(f"\nDataset shape: {df.shape}")
    print(f"Columns: {list(df.columns)}")
    
    # Step 2: Train Job Recommender
    print("\n" + "=" * 60)
    print("  STEP 2: Training Job Recommendation Engine")
    print("=" * 60)
    
    start = time.time()
    try:
        recommender = JobRecommender()
        recommender.train(df)
        recommender.save(save_dir)
        
        # Quick test
        test_result = recommender.recommend(["python", "django", "web development"], top_n=3)
        print(f"\n  Test: Top 3 recommendations for 'python django web development':")
        for r in test_result[:3]:
            print(f"    - {r['title'][:60]} (score: {r['similarity_score']})")
        
        elapsed = time.time() - start
        print(f"\n  Completed in {elapsed:.1f}s")
    except Exception as e:
        print(f"  ERROR: {e}")
    
    # Step 3: Train Budget Predictor
    print("\n" + "=" * 60)
    print("  STEP 3: Training Budget Predictor")
    print("=" * 60)
    
    start = time.time()
    try:
        budget_pred = BudgetPredictor()
        budget_pred.train(df)
        budget_pred.save(save_dir)
        
        # Quick test
        test_result = budget_pred.predict(
            title="Build a React website",
            description="Need a modern React.js website with dashboard and admin panel",
            category="web development",
            experience_level=2
        )
        print(f"\n  Test: Predicted budget for 'Build a React website': ${test_result['predicted_budget_usd']:.2f}")
        
        elapsed = time.time() - start
        print(f"\n  Completed in {elapsed:.1f}s")
    except Exception as e:
        print(f"  ERROR: {e}")
    
    # Step 4: Train Category Tagger
    print("\n" + "=" * 60)
    print("  STEP 4: Training Automated Category Tagger")
    print("=" * 60)
    
    start = time.time()
    try:
        tagger = CategoryTagger()
        tagger.train(df)
        tagger.save(save_dir)
        
        # Quick test
        test_result = tagger.predict(
            title="Design a mobile app UI",
            description="Looking for a designer to create beautiful mobile app screens in Figma"
        )
        print(f"\n  Test: Category for 'Design a mobile app UI': {test_result['predicted_category']} ({test_result['category_confidence']:.2%})")
        
        elapsed = time.time() - start
        print(f"\n  Completed in {elapsed:.1f}s")
    except Exception as e:
        print(f"  ERROR: {e}")
    
    # Step 5: Train Semantic Search
    print("\n" + "=" * 60)
    print("  STEP 5: Building Semantic Search Index")
    print("=" * 60)
    
    start = time.time()
    try:
        search = SemanticSearch(use_transformers=True)
        search.train(df)
        search.save(save_dir)
        
        # Quick test
        test_result = search.search("I need someone to build an e-commerce website", top_n=3)
        print(f"\n  Test: Search for 'e-commerce website':")
        for r in test_result[:3]:
            print(f"    - {r['title'][:60]} (score: {r['relevance_score']})")
        
        elapsed = time.time() - start
        print(f"\n  Completed in {elapsed:.1f}s")
    except Exception as e:
        print(f"  ERROR: {e}")
    
    # Step 6: Train Spam Detector
    print("\n" + "=" * 60)
    print("  STEP 6: Training Spam / Fake Job Detector")
    print("=" * 60)
    
    start = time.time()
    try:
        spam = SpamDetector()
        spam.train(df)
        spam.save(save_dir)
        
        # Quick test - normal posting
        test_normal = spam.check(
            title="Build a WordPress Website",
            description="Looking for an experienced WordPress developer to create a business website with contact form and blog section.",
            budget=500,
            experience_level=2,
            category="web development",
            location="United States"
        )
        print(f"\n  Test (normal job): Risk={test_normal['risk_level']}, Score={test_normal['risk_score']}")
        
        # Test - suspicious posting
        test_spam = spam.check(
            title="EARN $10000 FAST!!!",
            description="easy money contact me now 9999999999",
            budget=10000,
            experience_level=1,
            category="",
            location=""
        )
        print(f"  Test (suspicious job): Risk={test_spam['risk_level']}, Score={test_spam['risk_score']}")
        if test_spam['red_flags']:
            print(f"    Red flags: {', '.join(test_spam['red_flags'])}")
        
        elapsed = time.time() - start
        print(f"\n  Completed in {elapsed:.1f}s")
    except Exception as e:
        print(f"  ERROR: {e}")
    
    # Step 7: Train Skill Extractor
    print("\n" + "=" * 60)
    print("  STEP 7: Training Skill Extractor")
    print("=" * 60)
    
    start = time.time()
    try:
        extractor = SkillExtractor()
        extractor.train(df)
        extractor.save(save_dir)
        
        # Quick test
        test_text = "We need a Python developer with experience in Django, React, PostgreSQL, and Docker to build a REST API microservice."
        test_result = extractor.extract(test_text)
        skills = [s["skill"] for s in test_result["skills"]]
        print(f"\n  Test: Skills in '{test_text[:50]}...':")
        print(f"    Extracted: {', '.join(skills)}")
        
        elapsed = time.time() - start
        print(f"\n  Completed in {elapsed:.1f}s")
    except Exception as e:
        print(f"  ERROR: {e}")
    
    # Summary
    total_elapsed = time.time() - total_start
    print("\n" + "=" * 60)
    print("  TRAINING COMPLETE!")
    print("=" * 60)
    print(f"\n  Total time: {total_elapsed:.1f}s")
    print(f"  Models saved to: {save_dir}")
    print(f"\n  Saved model files:")
    
    if os.path.exists(save_dir):
        for f in sorted(os.listdir(save_dir)):
            size = os.path.getsize(os.path.join(save_dir, f))
            if size > 1024 * 1024:
                print(f"    {f} ({size / 1024 / 1024:.1f} MB)")
            else:
                print(f"    {f} ({size / 1024:.1f} KB)")
    
    print(f"\n  Next step: Run the API server with:")
    print(f"    cd ML_Models")
    print(f"    python -m api.main")
    
    return True


if __name__ == "__main__":
    train_all_models()
