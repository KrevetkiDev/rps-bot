FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/BotRps.Bot/BotRps.Bot.csproj", "src/BotRps.Bot/"]
COPY ["src/BotRps.Infrastructure/BotRps.Infrastructure.csproj", "src/BotRps.Infrastructure/"]
COPY ["src/BotRps.Application/BotRps.Application.csproj", "src/BotRps.Application/"]
COPY ["src/BotRps.Domain/BotRps.Domain.csproj", "src/BotRps.Domain/"]
RUN dotnet restore "src/BotRps.Bot/BotRps.Bot.csproj"
COPY . .
WORKDIR "/src/src/BotRps.Bot"
RUN dotnet build "BotRps.Bot.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BotRps.Bot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BotRps.Bot.dll"]
