FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Копируем файлы проекта и восстанавливаем зависимости
COPY ./*.csproj ./
RUN dotnet restore

# Копируем все файлы и собираем приложение
COPY . .
RUN dotnet publish -c Release -o out ./ticket.csproj

# Собираем финальный образ
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Копируем файлы конфигурации
COPY --from=build /app/out .
COPY ./appsettings*.json ./

ENTRYPOINT ["dotnet", "ticket.dll"]