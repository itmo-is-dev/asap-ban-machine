#!/bin/bash --login

pip install /packages/asap_ban_machine_model-0.0.1-py3-none-any.whl --force --no-deps
pip install /packages/asap_ban_machine_model-0.0.1-py3-none-any.whl

#while read requirement; do conda install --yes $requirement; done < requirements.txt