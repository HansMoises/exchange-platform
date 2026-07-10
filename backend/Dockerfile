# ===== Etapa 1: Build =====
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar archivos de proyecto y restaurar dependencias
COPY src/ExchangePlatform.API/ExchangePlatform.API.csproj src/ExchangePlatform.API/
COPY src/ExchangePlatform.Application/ExchangePlatform.Application.csproj src/ExchangePlatform.Application/
COPY src/ExchangePlatform.Domain/ExchangePlatform.Domain.csproj src/ExchangePlatform.Domain/
COPY src/ExchangePlatform.Infrastructure/ExchangePlatform.Infrastructure.csproj src/ExchangePlatform.Infrastructure/
RUN dotnet restore src/ExchangePlatform.API/ExchangePlatform.API.csproj

# Copiar todo el codigo fuente y compilar
COPY src/ ./src/
WORKDIR /src/src/ExchangePlatform.API
RUN dotnet publish -c Release -o /app/publish

# ===== Etapa 2: Runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Render inyecta la variable PORT. La app debe escuchar en ese puerto.
# El valor 10000 es solo un fallback por si PORT no viniera definido.
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "ExchangePlatform.API.dll"]
