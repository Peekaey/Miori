
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["*.sln", "./"]

COPY ["ShoukoV2.DiscordBot/ShoukoV2.DiscordBot.csproj", "ShoukoV2.DiscordBot/"]
COPY ["ShoukoV2.Models/ShoukoV2.Models.csproj", "ShoukoV2.Models/"]
COPY ["ShoukoV2.BackgroundService/ShoukoV2.BackgroundService.csproj", "ShoukoV2.BackgroundService/"]
COPY ["ShoukoV2.Helpers/ShoukoV2.Helpers.csproj", "ShoukoV2.Helpers/"]
COPY ["ShoukoV2.Integrations/ShoukoV2.Integrations.csproj", "ShoukoV2.Integrations/"]
COPY ["ShoukoV2.DataService/ShoukoV2.DataService.csproj", "ShoukoV2.DataService/"]
COPY ["ShoukoV2.Api/ShoukoV2.Api.csproj", "ShoukoV2.Api/"]
COPY ["ShoukoV2.BusinessService/ShoukoV2.BusinessService.csproj", "ShoukoV2.BusinessService/"]

RUN dotnet restore "ShoukoV2.DiscordBot/ShoukoV2.DiscordBot.csproj"

COPY . .

WORKDIR "/src/ShoukoV2.DiscordBot"
RUN dotnet build "ShoukoV2.DiscordBot.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ShoukoV2.DiscordBot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "ShoukoV2.DiscordBot.dll"]