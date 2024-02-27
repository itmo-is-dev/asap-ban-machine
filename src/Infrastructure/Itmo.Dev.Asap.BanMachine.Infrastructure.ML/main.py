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


def calculate_weighted_mean(scores):
    scores_array = np.array(scores)
    print(scores)

    weights = scores

    if sum(scores_array) == 0:
        return 0

    weighted_mean = np.average(scores_array, weights=weights)

    return weighted_mean


def serialize_node(node, source_bytes):
    if node.is_null():
        return None
    node_data = {
        'type': node.type,
        'start_byte': node.start_byte,
        'end_byte': node.end_byte,
        'start_point': {'row': node.start_point.row, 'column': node.start_point.column},
        'end_point': {'row': node.end_point.row, 'column': node.end_point.column},
        'is_named': node.is_named,
    }
    try:
        node_data['text'] = node.utf8_text(source_bytes).decode('utf-8')
    except AttributeError:
        pass
    children = []
    cursor = node.walk()
    if cursor.goto_first_child():
        while True:
            children.append(serialize_node(cursor.node, source_bytes))
            if not cursor.goto_next_sibling():
                break
    node_data['children'] = children
    return node_data


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
        with open(file1, 'rb') as f1:
            source_bytes1 = f1.read()

        for file2 in files2:
            if file2 in excluded2:
                continue

            with open(file2, 'rb') as f2:
                source_bytes2 = f2.read()

            print(f"\n[{counter}/{total_pair_count}] -- Comparing .cs files: \nfirst: {file1} \nsecond: {file2}")
            similarity, suspicious_blocks, _, _ = detector.compare_files(file1, file2, source_bytes1, source_bytes2)
            scores.append(similarity)

            all_suspicious_blocks.extend([(block, source_bytes1) for block in suspicious_blocks])

            counter += 1

            if similarity >= 0.9:
                print(f"Found similarity = {similarity}, excluding pair from further analysis")
                excluded2.add(file2)
                break

    return scores, all_suspicious_blocks


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
        serialized_blocks = [serialize_node(node, source_bytes) for node, source_bytes in suspicious_blocks_with_bytes]
        json.dump(serialized_blocks, f, indent=4)

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
