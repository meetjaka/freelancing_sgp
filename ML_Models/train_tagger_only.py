import sys
import os

# Add current directory to python path
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from training.data_preprocessor import get_preprocessed_data, get_saved_models_path
from models.category_tagger import CategoryTagger

if __name__ == "__main__":
    print("Loading data...")
    df = get_preprocessed_data("Freelance Platform Projects.csv")
    print("Training Category Tagger...")
    tagger = CategoryTagger()
    tagger.train(df)
    save_dir = get_saved_models_path()
    tagger.save(save_dir)
    print("Done! Files saved in:", save_dir)
