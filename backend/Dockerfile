FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

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
RUN dotnet publish "MilkiDrugStore.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MilkiDrugStore.Api.dll"]
