FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Squadmania.Squad.Rcon.Cli/Squadmania.Squad.Rcon.Cli.csproj", "Squadmania.Squad.Rcon.Cli/"]
RUN dotnet restore "Squadmania.Squad.Rcon.Cli/Squadmania.Squad.Rcon.Cli.csproj"
COPY . .
WORKDIR "/src/Squadmania.Squad.Rcon.Cli"
RUN dotnet build "Squadmania.Squad.Rcon.Cli.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Squadmania.Squad.Rcon.Cli.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Squadmania.Squad.Rcon.Cli.dll"]
