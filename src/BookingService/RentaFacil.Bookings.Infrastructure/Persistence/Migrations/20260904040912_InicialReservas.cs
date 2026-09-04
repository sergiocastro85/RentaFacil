using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentaFacil.Bookings.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InicialReservas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Documento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NombreCompleto = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReporteReservasDiarias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    TotalReservas = table.Column<int>(type: "int", nullable: false),
                    ValorTotalReservado = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TipoVehiculoMasReservado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ClientesUnicos = table.Column<int>(type: "int", nullable: false),
                    DetalleJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaProcesamiento = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReporteReservasDiarias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reservas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehiculoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoVehiculo = table.Column<int>(type: "int", nullable: false),
                    PlacaVehiculo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: false),
                    TarifaDiariaMonto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TarifaDiariaMoneda = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ValorTotalMonto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValorTotalMoneda = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservas_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "UX_Clientes_Documento",
                table: "Clientes",
                column: "Documento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_ReporteReservasDiarias_Fecha",
                table: "ReporteReservasDiarias",
                column: "Fecha",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_ClienteId",
                table: "Reservas",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_FechaCreacion",
                table: "Reservas",
                column: "FechaCreacion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReporteReservasDiarias");

            migrationBuilder.DropTable(
                name: "Reservas");

            migrationBuilder.DropTable(
                name: "Clientes");
        }
    }
}
