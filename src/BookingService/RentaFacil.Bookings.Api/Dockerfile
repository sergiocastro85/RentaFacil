FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["src/BookingService/RentaFacil.Bookings.Api/RentaFacil.Bookings.Api.csproj", "src/BookingService/RentaFacil.Bookings.Api/"]
COPY ["src/BookingService/RentaFacil.Bookings.Application/RentaFacil.Bookings.Application.csproj", "src/BookingService/RentaFacil.Bookings.Application/"]
COPY ["src/BookingService/RentaFacil.Bookings.Infrastructure/RentaFacil.Bookings.Infrastructure.csproj", "src/BookingService/RentaFacil.Bookings.Infrastructure/"]
COPY ["src/BookingService/RentaFacil.Bookings.Domain/RentaFacil.Bookings.Domain.csproj", "src/BookingService/RentaFacil.Bookings.Domain/"]
COPY ["src/Shared/RentaFacil.SharedKernel/RentaFacil.SharedKernel.csproj", "src/Shared/RentaFacil.SharedKernel/"]
RUN dotnet restore "src/BookingService/RentaFacil.Bookings.Api/RentaFacil.Bookings.Api.csproj"

COPY . .
WORKDIR "/src/src/BookingService/RentaFacil.Bookings.Api"
RUN dotnet build "RentaFacil.Bookings.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "RentaFacil.Bookings.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RentaFacil.Bookings.Api.dll"]
