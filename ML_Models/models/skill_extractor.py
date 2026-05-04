"""
Model 6: Job Skill Extraction
Hybrid approach: Rule-based dictionary matching + TF-IDF keyword extraction.

Automatically extracts key skills from job descriptions using a curated
dictionary of tech skills and statistical keyword importance.
"""

import os
import re
import json
import joblib
import numpy as np
from sklearn.feature_extraction.text import TfidfVectorizer
from collections import Counter


# Curated dictionary of common freelancing/tech skills (200+)
SKILLS_DICTIONARY = {
    # Programming Languages
    "python": "Python", "javascript": "JavaScript", "java": "Java",
    "c#": "C#", "csharp": "C#", "c++": "C++", "cpp": "C++",
    "php": "PHP", "ruby": "Ruby", "swift": "Swift", "kotlin": "Kotlin",
    "go": "Go", "golang": "Go", "rust": "Rust", "typescript": "TypeScript",
    "scala": "Scala", "r": "R", "matlab": "MATLAB", "perl": "Perl",
    "dart": "Dart", "lua": "Lua", "haskell": "Haskell", "elixir": "Elixir",
    "objective-c": "Objective-C", "assembly": "Assembly", "cobol": "COBOL",
    "fortran": "Fortran", "vba": "VBA", "bash": "Bash", "shell": "Shell Script",
    "powershell": "PowerShell", "sql": "SQL", "plsql": "PL/SQL",
    
    # Frontend Frameworks & Libraries
    "react": "React", "reactjs": "React", "react.js": "React",
    "angular": "Angular", "angularjs": "AngularJS",
    "vue": "Vue.js", "vuejs": "Vue.js", "vue.js": "Vue.js",
    "next.js": "Next.js", "nextjs": "Next.js",
    "nuxt": "Nuxt.js", "nuxtjs": "Nuxt.js",
    "svelte": "Svelte", "ember": "Ember.js",
    "jquery": "jQuery", "bootstrap": "Bootstrap",
    "tailwind": "Tailwind CSS", "tailwindcss": "Tailwind CSS",
    "material ui": "Material UI", "mui": "Material UI",
    "chakra": "Chakra UI", "ant design": "Ant Design",
    "redux": "Redux", "mobx": "MobX", "zustand": "Zustand",
    
    # Backend Frameworks
    "node.js": "Node.js", "nodejs": "Node.js", "node": "Node.js",
    "express": "Express.js", "expressjs": "Express.js",
    "django": "Django", "flask": "Flask", "fastapi": "FastAPI",
    "spring": "Spring", "spring boot": "Spring Boot",
    ".net": ".NET", "asp.net": "ASP.NET", "dotnet": ".NET",
    "laravel": "Laravel", "symfony": "Symfony",
    "rails": "Ruby on Rails", "ruby on rails": "Ruby on Rails",
    "nest.js": "NestJS", "nestjs": "NestJS",
    "koa": "Koa", "gin": "Gin", "fiber": "Fiber",
    
    # Mobile Development
    "react native": "React Native", "flutter": "Flutter",
    "android": "Android", "ios": "iOS",
    "xamarin": "Xamarin", "ionic": "Ionic",
    "swiftui": "SwiftUI", "jetpack compose": "Jetpack Compose",
    
    # Databases
    "mysql": "MySQL", "postgresql": "PostgreSQL", "postgres": "PostgreSQL",
    "mongodb": "MongoDB", "redis": "Redis", "sqlite": "SQLite",
    "oracle": "Oracle DB", "sql server": "SQL Server",
    "dynamodb": "DynamoDB", "cassandra": "Cassandra",
    "elasticsearch": "Elasticsearch", "neo4j": "Neo4j",
    "firebase": "Firebase", "supabase": "Supabase",
    "couchdb": "CouchDB", "mariadb": "MariaDB",
    
    # Cloud & DevOps
    "aws": "AWS", "amazon web services": "AWS",
    "azure": "Azure", "microsoft azure": "Azure",
    "gcp": "Google Cloud", "google cloud": "Google Cloud",
    "docker": "Docker", "kubernetes": "Kubernetes", "k8s": "Kubernetes",
    "terraform": "Terraform", "ansible": "Ansible",
    "jenkins": "Jenkins", "ci/cd": "CI/CD", "cicd": "CI/CD",
    "github actions": "GitHub Actions",
    "gitlab": "GitLab", "bitbucket": "Bitbucket",
    "nginx": "Nginx", "apache": "Apache",
    "heroku": "Heroku", "vercel": "Vercel", "netlify": "Netlify",
    "digitalocean": "DigitalOcean", "linode": "Linode",
    
    # AI & Data Science
    "machine learning": "Machine Learning", "ml": "Machine Learning",
    "deep learning": "Deep Learning",
    "artificial intelligence": "AI", "ai": "AI",
    "tensorflow": "TensorFlow", "pytorch": "PyTorch",
    "keras": "Keras", "scikit-learn": "Scikit-learn", "sklearn": "Scikit-learn",
    "numpy": "NumPy", "pandas": "Pandas",
    "nlp": "NLP", "natural language processing": "NLP",
    "computer vision": "Computer Vision", "opencv": "OpenCV",
    "data science": "Data Science", "data analysis": "Data Analysis",
    "data visualization": "Data Visualization",
    "tableau": "Tableau", "power bi": "Power BI",
    "jupyter": "Jupyter", "spark": "Apache Spark",
    "hadoop": "Hadoop", "airflow": "Apache Airflow",
    "llm": "LLM", "chatgpt": "ChatGPT", "openai": "OpenAI",
    "langchain": "LangChain", "hugging face": "Hugging Face",
    
    # Design & Creative
    "figma": "Figma", "sketch": "Sketch", "adobe xd": "Adobe XD",
    "photoshop": "Photoshop", "illustrator": "Illustrator",
    "after effects": "After Effects", "premiere pro": "Premiere Pro",
    "indesign": "InDesign", "canva": "Canva",
    "ui/ux": "UI/UX Design", "ux design": "UX Design", "ui design": "UI Design",
    "graphic design": "Graphic Design", "web design": "Web Design",
    "logo design": "Logo Design", "branding": "Branding",
    "3d modeling": "3D Modeling", "blender": "Blender",
    "unity": "Unity", "unreal engine": "Unreal Engine",
    
    # Web Technologies
    "html": "HTML", "html5": "HTML5", "css": "CSS", "css3": "CSS3",
    "sass": "SASS", "scss": "SCSS", "less": "LESS",
    "webpack": "Webpack", "vite": "Vite", "babel": "Babel",
    "graphql": "GraphQL", "rest api": "REST API", "restful": "REST API",
    "websocket": "WebSocket", "socket.io": "Socket.IO",
    "pwa": "Progressive Web App",
    
    # Testing
    "jest": "Jest", "mocha": "Mocha", "cypress": "Cypress",
    "selenium": "Selenium", "playwright": "Playwright",
    "pytest": "PyTest", "junit": "JUnit",
    "unit testing": "Unit Testing", "test automation": "Test Automation",
    
    # Project Management & Other
    "agile": "Agile", "scrum": "Scrum", "jira": "Jira",
    "git": "Git", "github": "GitHub",
    "seo": "SEO", "content writing": "Content Writing",
    "copywriting": "Copywriting", "technical writing": "Technical Writing",
    "wordpress": "WordPress", "shopify": "Shopify", "wix": "Wix",
    "woocommerce": "WooCommerce", "magento": "Magento",
    "blockchain": "Blockchain", "solidity": "Solidity",
    "web3": "Web3", "ethereum": "Ethereum", "smart contract": "Smart Contract",
    "api development": "API Development", "microservices": "Microservices",
    "oauth": "OAuth", "jwt": "JWT",
    "linux": "Linux", "windows server": "Windows Server",
    "networking": "Networking", "cybersecurity": "Cybersecurity",
    "penetration testing": "Penetration Testing",
    "data entry": "Data Entry", "virtual assistant": "Virtual Assistant",
    "excel": "Excel", "google sheets": "Google Sheets",
    "crm": "CRM", "salesforce": "Salesforce", "hubspot": "HubSpot",
    "erp": "ERP", "sap": "SAP",
}


class SkillExtractor:
    """Extracts key skills from job descriptions using dictionary + TF-IDF."""
    
    def __init__(self):
        self.skills_dict = SKILLS_DICTIONARY.copy()
        self.tfidf_vectorizer = None
        self.is_trained = False
        
        # Pre-compile regex patterns for multi-word skills (sorted by length, longest first)
        self._compiled_patterns = []
        sorted_skills = sorted(self.skills_dict.keys(), key=len, reverse=True)
        for skill_key in sorted_skills:
            # Escape special regex characters in the skill name
            escaped = re.escape(skill_key)
            pattern = re.compile(r"\b" + escaped + r"\b", re.IGNORECASE)
            self._compiled_patterns.append((pattern, self.skills_dict[skill_key]))
    
    def train(self, df):
        """
        Build TF-IDF model for keyword extraction (supplementary to dictionary).
        
        Args:
            df: Preprocessed DataFrame
        """
        print("[SkillExtractor] Training started...")
        
        texts = df["Description_clean"].fillna("").tolist()
        
        self.tfidf_vectorizer = TfidfVectorizer(
            max_features=5000,
            stop_words="english",
            ngram_range=(1, 2),
            min_df=3,
            max_df=0.9,
        )
        self.tfidf_vectorizer.fit(texts)
        
        # Analyze skill frequency across dataset
        all_skills = Counter()
        for text in texts:
            skills = self._match_dictionary(text)
            for skill in skills:
                all_skills[skill["skill"]] += 1
        
        self.skill_frequency = dict(all_skills.most_common(50))
        
        self.is_trained = True
        print(f"[SkillExtractor] Training complete.")
        print(f"[SkillExtractor] Top 10 skills in dataset: {dict(all_skills.most_common(10))}")
    
    def _match_dictionary(self, text):
        """Match skills from the dictionary against a text."""
        if not text:
            return []
        
        text_lower = text.lower()
        found_skills = {}
        
        for pattern, canonical_name in self._compiled_patterns:
            matches = pattern.findall(text_lower)
            if matches:
                if canonical_name not in found_skills:
                    found_skills[canonical_name] = len(matches)
                else:
                    found_skills[canonical_name] += len(matches)
        
        results = [
            {"skill": name, "count": count, "source": "dictionary"}
            for name, count in found_skills.items()
        ]
        
        return results
    
    def _extract_tfidf_keywords(self, text, top_n=10):
        """Extract important keywords using TF-IDF scores."""
        if not self.tfidf_vectorizer or not text:
            return []
        
        tfidf_vec = self.tfidf_vectorizer.transform([text])
        feature_names = self.tfidf_vectorizer.get_feature_names_out()
        
        # Get non-zero entries and their scores
        scores = tfidf_vec.toarray().flatten()
        top_indices = scores.argsort()[::-1][:top_n * 2]  # Get extra, filter later
        
        keywords = []
        for idx in top_indices:
            score = scores[idx]
            if score > 0:
                word = feature_names[idx]
                # Skip very short words and numbers
                if len(word) > 2 and not word.isdigit():
                    keywords.append({
                        "keyword": word,
                        "importance": round(float(score), 4),
                        "source": "tfidf",
                    })
        
        return keywords[:top_n]
    
    def extract(self, text, top_n=15):
        """
        Extract skills from a job description.
        
        Args:
            text: Job description text
            top_n: Maximum number of skills to return
            
        Returns:
            Dict with extracted skills and keywords
        """
        if not text:
            return {"skills": [], "keywords": [], "total_found": 0}
        
        # 1. Dictionary matching (high precision)
        dict_skills = self._match_dictionary(text)
        
        # Sort by count (most mentioned first)
        dict_skills.sort(key=lambda x: x["count"], reverse=True)
        
        # 2. TF-IDF keyword extraction (discover non-dictionary terms)
        tfidf_keywords = []
        if self.is_trained:
            tfidf_keywords = self._extract_tfidf_keywords(text.lower())
            
            # Filter out keywords that are already in dictionary results
            dict_skill_names = {s["skill"].lower() for s in dict_skills}
            tfidf_keywords = [
                kw for kw in tfidf_keywords 
                if kw["keyword"].lower() not in dict_skill_names
            ]
        
        return {
            "skills": dict_skills[:top_n],
            "keywords": tfidf_keywords[:10],
            "total_found": len(dict_skills),
        }
    
    def save(self, save_dir):
        """Save model artifacts to disk."""
        os.makedirs(save_dir, exist_ok=True)
        
        if self.tfidf_vectorizer:
            joblib.dump(self.tfidf_vectorizer, os.path.join(save_dir, "skill_tfidf_vectorizer.pkl"))
        
        # Save skills dictionary as JSON for easy editing
        with open(os.path.join(save_dir, "skills_dictionary.json"), "w", encoding="utf-8") as f:
            json.dump(self.skills_dict, f, indent=2)
        
        if hasattr(self, "skill_frequency"):
            joblib.dump(self.skill_frequency, os.path.join(save_dir, "skill_frequency.pkl"))
        
        print(f"[SkillExtractor] Model saved to {save_dir}")
    
    def load(self, save_dir):
        """Load model artifacts from disk."""
        tfidf_path = os.path.join(save_dir, "skill_tfidf_vectorizer.pkl")
        if os.path.exists(tfidf_path):
            self.tfidf_vectorizer = joblib.load(tfidf_path)
        
        dict_path = os.path.join(save_dir, "skills_dictionary.json")
        if os.path.exists(dict_path):
            with open(dict_path, "r", encoding="utf-8") as f:
                self.skills_dict = json.load(f)
            # Rebuild compiled patterns
            self._compiled_patterns = []
            sorted_skills = sorted(self.skills_dict.keys(), key=len, reverse=True)
            for skill_key in sorted_skills:
                escaped = re.escape(skill_key)
                pattern = re.compile(r"\b" + escaped + r"\b", re.IGNORECASE)
                self._compiled_patterns.append((pattern, self.skills_dict[skill_key]))
        
        freq_path = os.path.join(save_dir, "skill_frequency.pkl")
        if os.path.exists(freq_path):
            self.skill_frequency = joblib.load(freq_path)
        
        self.is_trained = True
        print(f"[SkillExtractor] Model loaded from {save_dir}")
