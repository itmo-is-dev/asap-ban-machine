FROM mcr.microsoft.com/dotnet/aspnet:7.0.5 AS base

RUN apt-get update && apt-get install -y wget bzip2

RUN wget https://repo.anaconda.com/miniconda/Miniconda3-latest-Linux-x86_64.sh -O ~/miniconda.sh && \
    bash ~/miniconda.sh -b -p /opt/conda && \
    rm ~/miniconda.sh

ENV PATH="/opt/conda/bin:${PATH}"