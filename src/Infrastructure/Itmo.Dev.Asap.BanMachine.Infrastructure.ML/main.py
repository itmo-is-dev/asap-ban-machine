import argparse
import os
import json
import zipfile
import tempfile
import shutil
import torch
import numpy as np
from asap_ban_machine_model.detector import (CodePlagiarismDetector)

detector = CodePlagiarismDetector()


def compare_directories(dir1, dir2):
    print(f"Comparing directories: {dir1} and {dir2}")

    scores = []
    all_suspicious_blocks = []

    files1 = [os.path.join(root, file) for root, dirs, files in os.walk(dir1) for file in files if file.endswith('.cs')]
    files2 = [os.path.join(root, file) for root, dirs, files in os.walk(dir2) for file in files if file.endswith('.cs')]

    total_pair_count = len(files1) * len(files2)
    counter = 1

    excluded2 = set()

    for file1 in files1:
        inner_counter = 0

        for file2 in files2:
            inner_counter += 1

            if file2 in excluded2:
                total_pair_count -= 1
                continue

            print(f"\n[{counter}/{total_pair_count})] -- Comparing .cs files: \nfirst: {file1} \nsecond: {file2}")
            similarity, suspicious_blocks, file1, file2 = detector.compare_files(file1, file2)
            scores.append(similarity)

            all_suspicious_blocks.extend(suspicious_blocks)
            counter += 1

            if similarity >= 0.9:
                print(f"Found similarity = {similarity}, excluding pair from further analysis")
                excluded2.add(file2)
                total_pair_count -= len(files2) - inner_counter
                break

    return scores, all_suspicious_blocks


def calculate_weighted_mean(scores):
    scores_array = np.array(scores)
    print(scores)

    weights = scores

    if sum(scores_array) == 0:
        return 0

    weighted_mean = np.average(scores_array, weights=weights)

    return weighted_mean


def compare_zip_files(zip1, zip2):
    print(f"Comparing zip files: {zip1} and {zip2}")
    dir1 = tempfile.mkdtemp()
    dir2 = tempfile.mkdtemp()

    if os.path.exists(args.result_dir):
        shutil.rmtree(args.result_dir)

    os.makedirs(args.result_dir)

    print(f"Extracting zip files to directories: {dir1} and {dir2}")
    with zipfile.ZipFile(zip1, 'r') as zip_ref:
        zip_ref.extractall(dir1)
    with zipfile.ZipFile(zip2, 'r') as zip_ref:
        zip_ref.extractall(dir2)

    scores, suspicious_blocks = compare_directories(dir1, dir2)

    # mean_score = statistics.mean(scores) if scores else 0
    mean_score = calculate_weighted_mean(scores)

    shutil.rmtree(dir1)
    shutil.rmtree(dir2)

    score_file = os.path.join(args.result_dir, 'similarity.txt')
    blocks_file = os.path.join(args.result_dir, 'suspicious_blocks.json')

    print(f"Writing mean score to file: {score_file}")

    with open(score_file, 'w') as f:
        f.write(str(mean_score))

    print(f"Writing block to file: {blocks_file}")

    with open(blocks_file, 'w') as f:
        json.dump(suspicious_blocks, f)

    return scores

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description='Compare zip files for file similarity.')
    parser.add_argument('zip1', type=str, help='First zip file to compare')
    parser.add_argument('zip2', type=str, help='Second zip file to compare')
    parser.add_argument('result_dir', type=str, help='File to store the results')
    parser.add_argument('num_cores', type=int, help='Physical core count')

    args = parser.parse_args()

    torch.set_num_threads(args.num_cores)

    compare_zip_files(args.zip1, args.zip2)
