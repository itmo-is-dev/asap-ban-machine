import argparse
import difflib
import os
import statistics
import json
import zipfile
import tempfile
import shutil


def compare_files(file1, file2):
    with open(file1, 'r') as f1, open(file2, 'r') as f2:
        text1 = f1.read()
        text2 = f2.read()

    lines1 = text1.splitlines()
    lines2 = text2.splitlines()

    seq = difflib.SequenceMatcher(None, lines1, lines2)
    ratio = seq.ratio()

    suspicious_blocks = []
    for tag, i1, i2, j1, j2 in seq.get_opcodes():
        if tag in ('replace', 'delete', 'insert'):
            method_block1 = extract_method_block(lines1, i1, i2)
            method_block2 = extract_method_block(lines2, j1, j2)

            block1 = {'FilePath': file1, 'LineFrom': method_block1['start'], 'LineTo': method_block1['end'],
                      'Content': method_block1['content']}
            block2 = {'FilePath': file2, 'LineFrom': method_block2['start'], 'LineTo': method_block2['end'],
                      'Content': method_block2['content']}
            if not any(block['First']['Content'] == method_block1['content'] and block['Second']['Content'] ==
                       method_block2['content'] for block in suspicious_blocks):
                suspicious_blocks.append({'First': block1, 'Second': block2, 'SimilarityScore': round(ratio, 2)})

    return ratio, suspicious_blocks


def extract_method_block(lines, start_idx, end_idx):
    start = start_idx
    while start > 0 and '{' not in lines[start]:
        start -= 1

    end = end_idx
    while end < len(lines) and '}' not in lines[end]:
        end += 1

    content = '\n'.join(lines[start:end + 1])

    return {'start': start + 1, 'end': end, 'content': content}


def unzip_directory(dir):
    for item in os.listdir(dir):
        if item.endswith('.zip'):
            file_name = os.path.join(dir, item)
            zip_ref = zipfile.ZipFile(file_name)
            zip_ref.extractall(dir)
            zip_ref.close()
            os.remove(file_name)


def compare_directories(dir1, dir2, similarity_file, suspicious_blocks_file):
    unzip_directory(dir1)
    unzip_directory(dir2)

    scores = []
    suspicious_blocks = []
    for root, dirs, files in os.walk(dir1):
        for file in files:
            if file.endswith('.cs'):
                rel_dir = os.path.relpath(root, dir1)
                rel_file = os.path.join(rel_dir, file)
                file1 = os.path.join(root, file)
                file2 = os.path.join(dir2, rel_file)
                if os.path.exists(file2):
                    similarity, blocks = compare_files(file1, file2)
                    scores.append(similarity)
                    suspicious_blocks.extend(blocks)

    mean_score = statistics.mean(scores)
    mean_score = str(round(mean_score, 2))

    with open(similarity_file, 'w') as f:
        f.write(mean_score)
    with open(suspicious_blocks_file, 'w') as f:
        json.dump(suspicious_blocks, f, indent=4)

    os.rmdir(dir1)
    os.rmdir(dir2)


def compare_zip_files(zip1, zip2, similarity_file, suspicious_blocks_file):
    dir1 = tempfile.mkdtemp()
    dir2 = tempfile.mkdtemp()

    with zipfile.ZipFile(zip1, 'r') as zip_ref:
        zip_ref.extractall(dir1)
    with zipfile.ZipFile(zip2, 'r') as zip_ref:
        zip_ref.extractall(dir2)

    compare_directories(dir1, dir2, similarity_file, suspicious_blocks_file)

    shutil.rmtree(dir1)
    shutil.rmtree(dir2)


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description='Compare zip files for file similarity.')
    parser.add_argument('zip1', type=str, help='First zip file to compare')
    parser.add_argument('zip2', type=str, help='Second zip file to compare')
    parser.add_argument('result_dir', type=str, help='Directory to store the results')

    args = parser.parse_args()

    if not os.path.exists(args.result_dir):
        os.makedirs(args.result_dir)

    similarity_file = os.path.join(args.result_dir, 'similarity.txt')
    suspicious_blocks_file = os.path.join(args.result_dir, 'suspicious_blocks.json')

    compare_zip_files(args.zip1, args.zip2, similarity_file, suspicious_blocks_file)
