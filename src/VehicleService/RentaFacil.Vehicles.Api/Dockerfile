FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["src/VehicleService/RentaFacil.Vehicles.Api/RentaFacil.Vehicles.Api.csproj", "src/VehicleService/RentaFacil.Vehicles.Api/"]
COPY ["src/VehicleService/RentaFacil.Vehicles.Application/RentaFacil.Vehicles.Application.csproj", "src/VehicleService/RentaFacil.Vehicles.Application/"]
COPY ["src/VehicleService/RentaFacil.Vehicles.Infrastructure/RentaFacil.Vehicles.Infrastructure.csproj", "src/VehicleService/RentaFacil.Vehicles.Infrastructure/"]
COPY ["src/VehicleService/RentaFacil.Vehicles.Domain/RentaFacil.Vehicles.Domain.csproj", "src/VehicleService/RentaFacil.Vehicles.Domain/"]
COPY ["src/Shared/RentaFacil.SharedKernel/RentaFacil.SharedKernel.csproj", "src/Shared/RentaFacil.SharedKernel/"]
RUN dotnet restore "src/VehicleService/RentaFacil.Vehicles.Api/RentaFacil.Vehicles.Api.csproj"

COPY . .
WORKDIR "/src/src/VehicleService/RentaFacil.Vehicles.Api"
RUN dotnet build "RentaFacil.Vehicles.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "RentaFacil.Vehicles.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RentaFacil.Vehicles.Api.dll"]
