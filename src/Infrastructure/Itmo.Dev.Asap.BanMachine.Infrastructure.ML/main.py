import argparse
import os
import json
import zipfile
import tempfile
import shutil
import torch
import numpy as np
from asap_ban_machine_model.detector import (CodePlagiarismDetector)
from pathlib import Path

detector = CodePlagiarismDetector()


def calculate_weighted_mean(scores):
    scores_array = np.array(scores)
    print(scores)

    weights = scores

    if sum(scores_array) == 0:
        return 0

    weighted_mean = np.average(scores_array, weights=weights)

    return weighted_mean


def clear_path(file_path):
    return os.path.join(*Path(file_path).parts[4:])


def serialize_node(node, source_bytes, file_path):
    content = source_bytes[node.start_byte:node.end_byte].decode('utf-8')
    return {
        "FilePath": file_path,
        "LineFrom": node.start_point[0] + 1,  # Tree-sitter line numbers are 0-based; adding 1 to match the contract
        "LineTo": node.end_point[0] + 1,
        "Content": content
    }


def compare_directories(dir1, dir2):
    print(f"Comparing directories: {dir1} and {dir2}")

    scores = []
    all_suspicious_blocks_info = []

    files1 = [os.path.join(root, file) for root, dirs, files in os.walk(dir1) for file in files if file.endswith('.cs')]
    files2 = [os.path.join(root, file) for root, dirs, files in os.walk(dir2) for file in files if file.endswith('.cs')]

    counter = 1
    total_pair_count = len(files1) * len(files2)
    excluded2 = set()

    for file1 in files1:
        with open(file1, 'rb') as f1:
            source_bytes1 = f1.read()

        for file2 in files2:
            if file2 in excluded2:
                continue
            with open(file2, 'rb') as f2:
                source_bytes2 = f2.read()

            print(f"\nComparing .cs files: \nfirst: {file1} \nsecond: {file2}")
            similarity, suspicious_blocks, _, _ = detector.compare_files(file1, file2, source_bytes1, source_bytes2)
            scores.append(similarity)

            for block in suspicious_blocks:
                all_suspicious_blocks_info.append({
                    "file1": file1,
                    "file2": file2,
                    "source_bytes1": source_bytes1,
                    "source_bytes2": source_bytes2,
                    "block": block
                })
            counter += 1
            #
            if similarity >= 0.9:
                print(f"Found similarity = {similarity}, excluding pair from further analysis")
                excluded2.add(file2)
                break

    return scores, all_suspicious_blocks_info


def compare_zip_files(zip1, zip2, result_dir):
    print(f"Comparing zip files: {zip1} and {zip2}")
    dir1 = tempfile.mkdtemp()
    dir2 = tempfile.mkdtemp()

    if os.path.exists(result_dir):
        shutil.rmtree(result_dir)
    os.makedirs(result_dir)

    print(f"Extracting zip files to directories: {dir1} and {dir2}")
    with zipfile.ZipFile(zip1, 'r') as zip_ref:
        zip_ref.extractall(dir1)
    with zipfile.ZipFile(zip2, 'r') as zip_ref:
        zip_ref.extractall(dir2)

    scores, suspicious_blocks_with_bytes = compare_directories(dir1, dir2)
    mean_score = calculate_weighted_mean(scores)

    shutil.rmtree(dir1)
    shutil.rmtree(dir2)

    score_file = os.path.join(result_dir, 'similarity.txt')
    blocks_file = os.path.join(result_dir, 'suspicious_blocks.json')

    print(f"Writing mean score to file: {score_file}")
    with open(score_file, 'w') as f:
        f.write(str(mean_score))

    print(f"Writing block to file: {blocks_file}")
    with open(blocks_file, 'w') as f:
        final_blocks = []
        for info in suspicious_blocks_with_bytes:
            block = info["block"]
            file1, file2 = info["file1"], info["file2"]
            source_bytes1, source_bytes2 = info["source_bytes1"], info["source_bytes2"]

            serialized_node1 = serialize_node(block['node1'], source_bytes1, clear_path(file1))
            serialized_node2 = serialize_node(block['node2'], source_bytes2, clear_path(file2))

            similarity = float(block['similarity']) if isinstance(block['similarity'], np.floating) else block[
                'similarity']

            final_blocks.append({
                "First": [serialized_node1],
                "Second": [serialized_node2],
                "SimilarityScore": similarity
            })
        json.dump(final_blocks, f, indent=4)

    return scores


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description='Compare zip files for file similarity.')
    parser.add_argument('zip1', type=str, help='First zip file to compare')
    parser.add_argument('zip2', type=str, help='Second zip file to compare')
    parser.add_argument('result_dir', type=str, help='File to store the results')
    parser.add_argument('num_cores', type=int, help='Physical core count')

    args = parser.parse_args()

    torch.set_num_threads(args.num_cores)

    compare_zip_files(args.zip1, args.zip2, args.result_dir)
