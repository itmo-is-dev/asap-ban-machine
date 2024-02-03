import argparse
import os
import json
import zipfile
import tempfile
import shutil
import torch
from asap_ban_machine_model.detector import (CodePlagiarismDetector)

detector = CodePlagiarismDetector()


def compare_directories(detector, dir1, dir2):
    print(f"Comparing directories: {dir1} and {dir2}")

    scores = []
    suspicious_blocks = []

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
            similarity, ast1, ast2, _, _ = detector.compare_files(file1, file2)
            scores.append(similarity)

            if similarity >= 0.2:
                suspicious_blocks.extend(detector.get_suspicious_blocks(ast1, ast2, similarity))

                print(f"Found similarity = {similarity}, excluding pair from further analysis")
                excluded2.add(file2)
                total_pair_count -= len(files2) - inner_counter
                break

            counter += 1

    return scores, suspicious_blocks


def compare_zip_files(detector, zip1, zip2, result_dir):
    print(f"Comparing zip files: {zip1} and {zip2}")
    dir1 = tempfile.mkdtemp()
    dir2 = tempfile.mkdtemp()

    print(f"Extracting zip files to directories: {dir1} and {dir2}")
    with zipfile.ZipFile(zip1, 'r') as zip_ref:
        zip_ref.extractall(dir1)
    with zipfile.ZipFile(zip2, 'r') as zip_ref:
        zip_ref.extractall(dir2)

    scores, suspicious_blocks = compare_directories(detector, dir1, dir2)
    mean_score = round(sum(scores) / len(scores), 2) if scores else 0

    shutil.rmtree(dir1)
    shutil.rmtree(dir2)

    if not os.path.exists(result_dir):
        os.makedirs(result_dir)

    score_file = os.path.join(result_dir, 'similarity.txt')
    blocks_file = os.path.join(result_dir, 'suspicious_blocks.json')

    print(f"Writing mean score to file: {score_file}")

    with open(score_file, 'w') as f:
        f.write(str(mean_score))

    print(f"Writing suspicious blocks to file: {blocks_file}")

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
