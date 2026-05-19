# ============================================
# CorporateSystem — Multi-stage Dockerfile
# ============================================

# Stage 1: Build, Test & Publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем файлы проектов для кэширования restore
COPY CorporateSystem/CorporateSystem.csproj CorporateSystem/
COPY CorporateSystem.Client/CorporateSystem.Client.csproj CorporateSystem.Client/
COPY CorporateSystem.Shared/CorporateSystem.Shared.csproj CorporateSystem.Shared/
COPY CorporateSystem.Tests/CorporateSystem.Tests.csproj CorporateSystem.Tests/
COPY CorporateSystem.sln .

# Восстанавливаем зависимости
RUN dotnet restore CorporateSystem.sln

# Копируем весь исходный код
COPY . .

# Запускаем тесты — если упадут, сборка прервётся
RUN dotnet test CorporateSystem.Tests/CorporateSystem.Tests.csproj \
    -c Release --no-restore --verbosity minimal

# Публикуем приложение
RUN dotnet publish CorporateSystem/CorporateSystem.csproj \
    -c Release -o /app/publish --no-restore

# Stage 2: Runtime (минимальный образ)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Копируем опубликованное приложение
COPY --from=build /app/publish .

# Настройка переменных окружения
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "CorporateSystem.dll"]
