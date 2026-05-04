"""
Data Preprocessor for ML Models
Handles loading, cleaning, and preparing the freelancing dataset for all models.
"""

import pandas as pd
import numpy as np
import re
import os

# Static currency conversion rates to USD (approximate)
CURRENCY_TO_USD = {
    "USD": 1.0,
    "$": 1.0,
    "EUR": 1.10,
    "€": 1.10,
    "GBP": 1.27,
    "£": 1.27,
    "INR": 0.012,
    "₹": 0.012,
    "CAD": 0.74,
    "AUD": 0.65,
    "SGD": 0.74,
    "AED": 0.27,
    "PKR": 0.0036,
    "BDT": 0.0091,
    "PHP": 0.018,
    "MYR": 0.21,
    "NGN": 0.00065,
    "KES": 0.0065,
    "ZAR": 0.053,
    "BRL": 0.20,
    "MXN": 0.058,
    "JPY": 0.0067,
    "CNY": 0.14,
    "KRW": 0.00075,
    "THB": 0.028,
    "VND": 0.000041,
    "IDR": 0.000063,
    "TRY": 0.031,
    "RUB": 0.011,
    "UAH": 0.025,
    "PLN": 0.25,
    "CZK": 0.044,
    "HUF": 0.0028,
    "RON": 0.22,
    "SEK": 0.096,
    "NOK": 0.094,
    "DKK": 0.15,
    "CHF": 1.13,
    "NZD": 0.61,
    "HKD": 0.13,
    "TWD": 0.031,
    "CLP": 0.0011,
    "COP": 0.00025,
    "ARS": 0.0012,
    "EGP": 0.021,
    "SAR": 0.27,
    "QAR": 0.27,
    "KWD": 3.26,
    "BHD": 2.65,
    "OMR": 2.60,
    "JOD": 1.41,
    "LKR": 0.0031,
    "NPR": 0.0075,
}

# Experience level mapping
EXPERIENCE_MAP = {
    "entry": 1,
    "entry level": 1,
    "entry-level": 1,
    "junior": 1,
    "beginner": 1,
    "intern": 1,
    "intermediate": 2,
    "mid": 2,
    "mid level": 2,
    "mid-level": 2,
    "expert": 3,
    "senior": 3,
    "advanced": 3,
    "lead": 3,
}


def get_data_path():
    """Get the path to the data directory."""
    base_dir = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    return os.path.join(base_dir, "data")


def get_saved_models_path():
    """Get the path to the saved models directory."""
    base_dir = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    return os.path.join(base_dir, "saved_models")


def load_dataset(filename="Freelance Platform Projects.csv"):
    """Load the raw CSV dataset."""
    data_path = os.path.join(get_data_path(), filename)
    if not os.path.exists(data_path):
        raise FileNotFoundError(
            f"Dataset not found at {data_path}. "
            f"Please place your CSV file in the 'ML_Models/data/' directory."
        )
    
    df = pd.read_csv(data_path, encoding="utf-8", on_bad_lines="skip")
    print(f"[Preprocessor] Loaded {len(df)} rows from {filename}")
    return df


def clean_text(text):
    """Clean a single text string."""
    if pd.isna(text) or not isinstance(text, str):
        return ""
    # Convert to lowercase
    text = text.lower().strip()
    # Remove HTML tags
    text = re.sub(r"<[^>]+>", " ", text)
    # Remove URLs
    text = re.sub(r"http\S+|www\.\S+", " ", text)
    # Remove special characters but keep spaces and basic punctuation
    text = re.sub(r"[^a-zA-Z0-9\s\.\,\-\/\+\#]", " ", text)
    # Collapse multiple spaces
    text = re.sub(r"\s+", " ", text).strip()
    return text


def normalize_column_names(df):
    """
    Normalize column names to a standard format.
    Handles variations in column naming from different CSV exports.
    """
    # Create a mapping of possible column names to standard names
    column_mapping = {}
    standard_names = {
        "title": "Title",
        "category name": "Category_Name",
        "category_name": "Category_Name",
        "categoryname": "Category_Name",
        "experience": "Experience",
        "sub category": "Sub_Category",
        "sub_category": "Sub_Category",
        "subcategory": "Sub_Category",
        "sub category name": "Sub_Category",
        "sub_category_name": "Sub_Category",
        "currency": "Currency",
        "budget": "Budget",
        "location": "Location",
        "freelance type": "Freelance_Type",
        "freelance_type": "Freelance_Type",
        "freelancetype": "Freelance_Type",
        "freelancer preferred from": "Freelance_Type",
        "type": "Freelance_Type",
        "date post": "Date_Post",
        "date_post": "Date_Post",
        "datepost": "Date_Post",
        "date posted": "Date_Post",
        "date_posted": "Date_Post",
        "description": "Description",
        "duration": "Duration",
        "client reg": "Client_Reg",
        "client_reg": "Client_Reg",
        "clientreg": "Client_Reg",
        "client registration": "Client_Reg",
        "client registration date": "Client_Reg",
        "client_registration_date": "Client_Reg",
        "client city": "Client_City",
        "client_city": "Client_City",
        "clientcity": "Client_City",
        "client country": "Client_Country",
        "client_country": "Client_Country",
        "clientcountry": "Client_Country",
        "client currency": "Client_Currency",
        "client_currency": "Client_Currency",
        "clientcurrency": "Client_Currency",
        "client job title": "Client_Job_Title",
        "client_job_title": "Client_Job_Title",
        "clientjobtitle": "Client_Job_Title",
        "sub categoy": "Sub_Category",  # common typo
    }
    
    for col in df.columns:
        normalized = col.strip().lower()
        if normalized in standard_names:
            column_mapping[col] = standard_names[normalized]
        else:
            # Keep original but clean up spaces/special chars
            column_mapping[col] = col.strip().replace(" ", "_")
    
    df = df.rename(columns=column_mapping)
    return df


def parse_experience(exp_str):
    """Convert experience string to numeric level (1=Entry, 2=Intermediate, 3=Expert)."""
    if pd.isna(exp_str) or not isinstance(exp_str, str):
        return 2  # Default to intermediate
    
    exp_lower = exp_str.strip().lower()
    for key, value in EXPERIENCE_MAP.items():
        if key in exp_lower:
            return value
    return 2  # Default to intermediate


def normalize_budget(budget, currency):
    """Convert a budget amount to USD using the currency conversion map."""
    if pd.isna(budget) or budget == 0:
        return 0.0
    
    if pd.isna(currency) or not isinstance(currency, str):
        return float(budget)  # Assume USD if unknown
    
    currency_clean = currency.strip().upper()
    
    # Try direct match
    rate = CURRENCY_TO_USD.get(currency_clean, None)
    
    if rate is None:
        # Try matching by first 3 characters (ISO code)
        for key, val in CURRENCY_TO_USD.items():
            if key.upper() in currency_clean or currency_clean in key.upper():
                rate = val
                break
    
    if rate is None:
        rate = 1.0  # Default to USD if currency not found
    
    return float(budget) * rate


def parse_budget_string(budget_str):
    """Parse budget from string format (e.g., '$500', '100-200', '1,000')."""
    if pd.isna(budget_str):
        return 0.0
    
    if isinstance(budget_str, (int, float)):
        return float(budget_str)
    
    budget_str = str(budget_str).strip()
    
    # Remove currency symbols and commas
    budget_str = re.sub(r"[£€₹$,]", "", budget_str)
    
    # Handle ranges like "100-200" — take the average
    range_match = re.match(r"(\d+\.?\d*)\s*[-–to]+\s*(\d+\.?\d*)", budget_str)
    if range_match:
        low = float(range_match.group(1))
        high = float(range_match.group(2))
        return (low + high) / 2
    
    # Try to extract first number
    num_match = re.search(r"(\d+\.?\d*)", budget_str)
    if num_match:
        return float(num_match.group(1))
    
    return 0.0


def preprocess_dataset(df):
    """
    Full preprocessing pipeline.
    Returns a clean DataFrame ready for model training.
    """
    print("[Preprocessor] Starting data preprocessing...")
    
    # 1. Normalize column names
    df = normalize_column_names(df)
    print(f"[Preprocessor] Columns after normalization: {list(df.columns)}")
    
    # 2. Drop rows with no Title
    if "Title" in df.columns:
        df = df.dropna(subset=["Title"])
        df = df[df["Title"].str.strip() != ""]
    
    # 3. Clean text fields
    text_columns = ["Title", "Description", "Category_Name", "Sub_Category", 
                     "Location", "Client_City", "Client_Country", "Client_Job_Title"]
    for col in text_columns:
        if col in df.columns:
            df[col + "_clean"] = df[col].apply(clean_text)
    
    # 4. Parse Experience to numeric
    if "Experience" in df.columns:
        df["Experience_Level"] = df["Experience"].apply(parse_experience)
    else:
        df["Experience_Level"] = 2
    
    # 5. Parse and normalize Budget
    if "Budget" in df.columns:
        df["Budget_Parsed"] = df["Budget"].apply(parse_budget_string)
    else:
        df["Budget_Parsed"] = 0.0
    
    currency_col = "Currency" if "Currency" in df.columns else "Client_Currency"
    if currency_col in df.columns:
        df["Budget_USD"] = df.apply(
            lambda row: normalize_budget(row["Budget_Parsed"], row.get(currency_col, "USD")),
            axis=1
        )
    else:
        df["Budget_USD"] = df["Budget_Parsed"]
    
    # 6. Create combined text feature for ML models
    parts = []
    for col in ["Title_clean", "Description_clean", "Category_Name_clean", "Sub_Category_clean"]:
        if col in df.columns:
            parts.append(df[col].fillna(""))
    
    if parts:
        df["combined_text"] = parts[0]
        for part in parts[1:]:
            df["combined_text"] = df["combined_text"] + " " + part
        df["combined_text"] = df["combined_text"].str.strip()
    else:
        df["combined_text"] = ""
    
    # 7. Fill remaining NaN values
    df["Description_clean"] = df.get("Description_clean", pd.Series([""] * len(df))).fillna("")
    df["Category_Name_clean"] = df.get("Category_Name_clean", pd.Series([""] * len(df))).fillna("")
    df["Sub_Category_clean"] = df.get("Sub_Category_clean", pd.Series([""] * len(df))).fillna("")
    df["Title_clean"] = df.get("Title_clean", pd.Series([""] * len(df))).fillna("")
    
    # 8. Remove duplicates
    initial_count = len(df)
    if "Title_clean" in df.columns and "Description_clean" in df.columns:
        df = df.drop_duplicates(subset=["Title_clean", "Description_clean"], keep="first")
    print(f"[Preprocessor] Removed {initial_count - len(df)} duplicate rows")
    
    # 9. Reset index
    df = df.reset_index(drop=True)
    
    print(f"[Preprocessor] Preprocessing complete. Final shape: {df.shape}")
    return df


def get_preprocessed_data(filename="Freelance Platform Projects.csv"):
    """Convenience function: load + preprocess in one call."""
    df = load_dataset(filename)
    df = preprocess_dataset(df)
    return df


if __name__ == "__main__":
    # Test preprocessing
    try:
        df = get_preprocessed_data()
        print("\n=== Dataset Info ===")
        print(f"Shape: {df.shape}")
        print(f"Columns: {list(df.columns)}")
        print(f"\nSample combined_text:")
        print(df["combined_text"].head(3).tolist())
        print(f"\nBudget USD stats:")
        print(df["Budget_USD"].describe())
        print(f"\nExperience distribution:")
        print(df["Experience_Level"].value_counts())
    except FileNotFoundError as e:
        print(f"Error: {e}")
