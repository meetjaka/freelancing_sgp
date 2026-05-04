# SGP Freelancing - ML Models

Machine Learning models for the SGP Freelancing Platform. Provides 6 intelligent features powered by AI.

## Models

| # | Model | Algorithm | Endpoint |
|---|-------|-----------|----------|
| 1 | Job Recommendation Engine | TF-IDF + Cosine Similarity | `POST /recommend` |
| 2 | Budget Predictor | Gradient Boosting Regressor | `POST /predict-budget` |
| 3 | Category Auto-Tagger | Linear SVC Classifier | `POST /predict-category` |
| 4 | Semantic Search | Sentence Transformers / TF-IDF | `POST /search` |
| 5 | Spam / Fake Job Detector | Isolation Forest | `POST /check-spam` |
| 6 | Skill Extractor | Dictionary + TF-IDF | `POST /extract-skills` |

## Setup

### 1. Create Virtual Environment

```bash
cd ML_Models
python -m venv venv
venv\Scripts\activate    # Windows
# source venv/bin/activate  # Linux/Mac
```

### 2. Install Dependencies

```bash
pip install -r requirements.txt
```

### 3. Add Dataset

Place your CSV file in the `data/` folder:
```
ML_Models/data/Freelance Platform Projects.csv
```

### 4. Train All Models

```bash
python -m training.train_all
```

This will:
- Load and preprocess the dataset
- Train all 6 models
- Save trained models to `saved_models/`
- Show test results for each model

### 5. Start the API Server

```bash
python -m api.main
```

The API will start on `http://localhost:8001`

### 6. View API Documentation

Open `http://localhost:8001/docs` in your browser for interactive Swagger documentation.

## API Endpoints

### Health Check
```
GET /health
```

### Job Recommendations
```json
POST /recommend
{
    "skills": ["python", "django", "react"],
    "bio": "Full-stack developer with 3 years experience",
    "top_n": 10
}
```

### Budget Prediction
```json
POST /predict-budget
{
    "title": "Build a React website",
    "description": "Modern website with dashboard",
    "category": "web development",
    "experience_level": 2
}
```

### Category Tagging
```json
POST /predict-category
{
    "title": "Design a mobile app UI",
    "description": "Create beautiful mobile app screens in Figma"
}
```

### Semantic Search
```json
POST /search
{
    "query": "e-commerce website development",
    "top_n": 10
}
```

### Spam Detection
```json
POST /check-spam
{
    "title": "Build a WordPress Site",
    "description": "Need a professional WordPress website...",
    "budget": 500,
    "experience_level": 2,
    "category": "web development",
    "location": "India"
}
```

### Skill Extraction
```json
POST /extract-skills
{
    "text": "Need Python developer with Django, React, and PostgreSQL experience",
    "top_n": 15
}
```

## Folder Structure

```
ML_Models/
├── data/                          # Dataset CSV files
├── models/                        # Model class definitions
│   ├── job_recommender.py
│   ├── budget_predictor.py
│   ├── category_tagger.py
│   ├── semantic_search.py
│   ├── spam_detector.py
│   └── skill_extractor.py
├── training/                      # Data processing & training
│   ├── data_preprocessor.py
│   └── train_all.py
├── saved_models/                  # Serialized trained models (.pkl)
├── api/                           # FastAPI server
│   ├── main.py
│   └── schemas.py
├── requirements.txt
└── README.md
```
