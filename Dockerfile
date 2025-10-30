
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["*.sln", "./"]

COPY ["Miori.DiscordBot/Miori.DiscordBot.csproj", "Miori.DiscordBot/"]
COPY ["Miori.Models/Miori.Models.csproj", "Miori.Models/"]
COPY ["Miori.BackgroundService/Miori.BackgroundService.csproj", "Miori.BackgroundService/"]
COPY ["Miori.Helpers/Miori.Helpers.csproj", "Miori.Helpers/"]
COPY ["Miori.Integrations/Miori.Integrations.csproj", "Miori.Integrations/"]
COPY ["Miori.DataService/Miori.DataService.csproj", "Miori.DataService/"]
COPY ["Miori.Api/Miori.Api.csproj", "Miori.Api/"]
COPY ["Miori.BusinessService/Miori.BusinessService.csproj", "Miori.BusinessService/"]

RUN dotnet restore "Miori.DiscordBot/Miori.DiscordBot.csproj"

COPY . .

WORKDIR "/src/Miori.DiscordBot"
RUN dotnet build "Miori.DiscordBot.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Miori.DiscordBot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Miori.DiscordBot.dll"]