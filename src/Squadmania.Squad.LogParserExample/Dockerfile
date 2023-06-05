FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Squadmania.Squad.LogParserExample/Squadmania.Squad.LogParserExample.csproj", "Squadmania.Squad.LogParserExample/"]
RUN dotnet restore "Squadmania.Squad.LogParserExample/Squadmania.Squad.LogParserExample.csproj"
COPY . .
WORKDIR "/src/Squadmania.Squad.LogParserExample"
RUN dotnet build "Squadmania.Squad.LogParserExample.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Squadmania.Squad.LogParserExample.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Squadmania.Squad.LogParserExample.dll"]
