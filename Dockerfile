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
  && apt-get install -y python3-pip python3-dev \
  && cd /usr/local/bin \
  && ln -s /usr/bin/python3 python \
  && pip3 install --upgrade pip 

RUN pip install -r requirements.txt

ENTRYPOINT ["dotnet", "Itmo.Dev.Asap.BanMachine.dll"]