#!/bin/bash

ENV_NAME="asap-ban-machine"
conda init
conda env list | awk '{print $1}' | grep -w $ENV_NAME
if [ $? -eq 0 ]; then
    echo "Environment $ENV_NAME exists, activating it"
    conda activate $ENV_NAME
else
    echo "Environment $ENV_NAME does not exist, creating it"
    conda create -n $ENV_NAME
    conda activate $ENV_NAME
fi

conda install --file /packages/asap-ban-machine-model.whl

#while read requirement; do conda install --yes $requirement; done < requirements.txt