FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["src/BookingService/RentaFacil.Reporting.Worker/RentaFacil.Reporting.Worker.csproj", "src/BookingService/RentaFacil.Reporting.Worker/"]
COPY ["src/BookingService/RentaFacil.Bookings.Application/RentaFacil.Bookings.Application.csproj", "src/BookingService/RentaFacil.Bookings.Application/"]
COPY ["src/BookingService/RentaFacil.Bookings.Infrastructure/RentaFacil.Bookings.Infrastructure.csproj", "src/BookingService/RentaFacil.Bookings.Infrastructure/"]
COPY ["src/BookingService/RentaFacil.Bookings.Domain/RentaFacil.Bookings.Domain.csproj", "src/BookingService/RentaFacil.Bookings.Domain/"]
COPY ["src/Shared/RentaFacil.SharedKernel/RentaFacil.SharedKernel.csproj", "src/Shared/RentaFacil.SharedKernel/"]
RUN dotnet restore "src/BookingService/RentaFacil.Reporting.Worker/RentaFacil.Reporting.Worker.csproj"

COPY . .
WORKDIR "/src/src/BookingService/RentaFacil.Reporting.Worker"
RUN dotnet build "RentaFacil.Reporting.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "RentaFacil.Reporting.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RentaFacil.Reporting.Worker.dll"]
