FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/MilkiDrugStore.Api/MilkiDrugStore.Api.csproj", "src/MilkiDrugStore.Api/"]
COPY ["src/MilkiDrugStore.Application/MilkiDrugStore.Application.csproj", "src/MilkiDrugStore.Application/"]
COPY ["src/MilkiDrugStore.Domain/MilkiDrugStore.Domain.csproj", "src/MilkiDrugStore.Domain/"]
COPY ["src/MilkiDrugStore.Persistence/MilkiDrugStore.Persistence.csproj", "src/MilkiDrugStore.Persistence/"]
COPY ["src/MilkiDrugStore.Infrastructure/MilkiDrugStore.Infrastructure.csproj", "src/MilkiDrugStore.Infrastructure/"]

RUN dotnet restore "src/MilkiDrugStore.Api/MilkiDrugStore.Api.csproj"

COPY . .
WORKDIR /src/src/MilkiDrugStore.Api
RUN dotnet build "MilkiDrugStore.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MilkiDrugStore.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

# Create data directories on the persistent disk mount point.
# Render mounts the Persistent Disk at /var/data, so keys stored under
# /var/data/keys survive container restarts and redeployments.
RUN mkdir -p /var/data/keys && \
    chmod 755 /var/data && \
    chmod 755 /var/data/keys

COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "MilkiDrugStore.Api.dll"]
