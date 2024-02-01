import argparse
import os
import json
import zipfile
import tempfile
import shutil
from asap_ban_machine_model.detector import (CodePlagiarismDetector)

detector = CodePlagiarismDetector()


def compare_directories(dir1, dir2):
    print(f"Comparing directories: {dir1} and {dir2}")
    scores = []
    files1 = [os.path.join(root, file) for root, dirs, files in os.walk(dir1) for file in files if file.endswith('.cs')]
    files2 = [os.path.join(root, file) for root, dirs, files in os.walk(dir2) for file in files if file.endswith('.cs')]
    for file1 in files1:
        for file2 in files2:
            print(f"Comparing .cs files: {file1} and {file2}")
            similarity = detector.compare_files(file1, file2)
            scores.append(similarity)
    return scores


def compare_zip_files(zip1, zip2):
    print(f"Comparing zip files: {zip1} and {zip2}")
    dir1 = tempfile.mkdtemp()
    dir2 = tempfile.mkdtemp()

    print(f"Extracting zip files to directories: {dir1} and {dir2}")
    with zipfile.ZipFile(zip1, 'r') as zip_ref:
        zip_ref.extractall(dir1)
    with zipfile.ZipFile(zip2, 'r') as zip_ref:
        zip_ref.extractall(dir2)

    scores = compare_directories(dir1, dir2)
    shutil.rmtree(dir1)
    shutil.rmtree(dir2)
    return scores


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description='Compare zip files for file similarity.')
    parser.add_argument('zip1', type=str, help='First zip file to compare')
    parser.add_argument('zip2', type=str, help='Second zip file to compare')
    parser.add_argument('result_dir', type=str, help='File to store the results')

    args = parser.parse_args()

    scores = compare_zip_files(args.zip1, args.zip2)
    mean_score = sum(scores) / len(scores) if scores else 0

    if not os.path.exists(args.result_dir):
        os.makedirs(args.result_dir)

    score_file = os.path.join(args.result_dir, 'similarity.txt')
    blocks_file = os.path.join(args.result_dir, 'suspicious_blocks.json')

    print(f"Writing mean score to file: {score_file}")

    with open(score_file, 'w') as f:
        f.write(str(mean_score))

    print(f"Writing block to file: {blocks_file}")

    with open(blocks_file, 'w') as f:
        f.write('[]')
