FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

RUN apt-get update && apt-get install -y curl ca-certificates

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ./*.props ./

COPY ["src/RomanTourNotification/RomanTourNotification.csproj", "src/RomanTourNotification/"]
COPY ["src/Application/RomanTourNotification.Application.Models/RomanTourNotification.Application.Models.csproj", "src/Application/RomanTourNotification.Application.Models/"]
COPY ["src/Presentation/RomanTourNotification.Presentation.TelegramBot/RomanTourNotification.Presentation.TelegramBot.csproj", "src/Presentation/RomanTourNotification.Presentation.TelegramBot/"]
COPY ["src/Application/RomanTourNotification.Application.Contracts/RomanTourNotification.Application.Contracts.csproj", "src/Application/RomanTourNotification.Application.Contracts/"]
COPY ["src/Application/RomanTourNotification.Application.Abstractions/RomanTourNotification.Application.Abstractions.csproj", "src/Application/RomanTourNotification.Application.Abstractions/"]
COPY ["src/Application/RomanTourNotification.Application/RomanTourNotification.Application.csproj", "src/Application/RomanTourNotification.Application/"]
COPY ["src/Infrastructure/RomanTourNotification.Infrastructure.Persistence/RomanTourNotification.Infrastructure.Persistence.csproj", "src/Infrastructure/RomanTourNotification.Infrastructure.Persistence/"]

RUN dotnet restore "src/RomanTourNotification/RomanTourNotification.csproj"

COPY . .
WORKDIR "/src/src/RomanTourNotification"
RUN dotnet build "RomanTourNotification.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "RomanTourNotification.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "RomanTourNotification.dll"]