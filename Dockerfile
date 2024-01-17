FROM mcr.microsoft.com/dotnet/sdk:7.0.203 AS build
WORKDIR /source
COPY ./src ./src
COPY ./*.sln .
COPY ./*.props ./
COPY ./.editorconfig .

RUN dotnet restore "src/Itmo.Dev.Asap.BanMachine/Itmo.Dev.Asap.BanMachine.csproj"

FROM build AS publish
WORKDIR "/source/src/Itmo.Dev.Asap.BanMachine"
RUN dotnet publish "Itmo.Dev.Asap.BanMachine.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:7.0.5 AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        software-properties-common \
        dirmngr \
        gnupg2 \
    && add-apt-repository ppa:deadsnakes/ppa \
    && apt-get update \
    && apt-get install -y --no-install-recommends python3.9=3.9.18-1+bionic1 python3.9-distutils \
    && apt-get remove --purge -y software-properties-common dirmngr gnupg2 \
    && apt-get autoremove -y \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

RUN ln -s /usr/bin/python3.9 /usr/bin/python3

ENTRYPOINT ["dotnet", "Itmo.Dev.Asap.BanMachine.dll"]