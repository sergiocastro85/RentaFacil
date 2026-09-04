using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentaFacil.Vehicles.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InicialVehiculos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Vehiculos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Placa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Marca = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Modelo = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Anio = table.Column<int>(type: "int", nullable: false),
                    TarifaDiariaMonto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TarifaDiariaMoneda = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehiculos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BloqueosDisponibilidad",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehiculoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: false),
                    ReferenciaExternaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BloqueosDisponibilidad", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BloqueosDisponibilidad_Vehiculos_VehiculoId",
                        column: x => x.VehiculoId,
                        principalTable: "Vehiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Índice de FK de una sola columna sobre VehiculoId, creado por convención
            // de EF Core porque el modelo C# no puede declarar el índice compuesto de
            // abajo (ver comentario). Se mantiene aunque quede redundante con ese índice
            // compuesto: si se elimina de aquí sin eliminarlo también del modelo, un
            // futuro "dotnet ef migrations add" lo detecta como un cambio pendiente y
            // propone recrearlo, generando una migración espuria que habría que editar
            // a mano. Es preferible un índice redundante a un modelo desincronizado.
            migrationBuilder.CreateIndex(
                name: "IX_BloqueosDisponibilidad_VehiculoId",
                table: "BloqueosDisponibilidad",
                column: "VehiculoId");

            // Índice compuesto de ARCHITECTURE.md §3.1. EF Core no soporta declarar en
            // Fluent API un índice que combine una columna del propietario (VehiculoId)
            // con propiedades de un owned type (Periodo.FechaInicio/FechaFin) mapeado a
            // la misma tabla, así que se agrega con DDL directo. El modelo C# no conoce
            // este índice: al regenerar migraciones hay que confirmar a mano que sigue
            // presente en el archivo generado.
            // Ver: https://github.com/dotnet/efcore/issues/11336
            migrationBuilder.CreateIndex(
                name: "IX_Bloqueos_VehiculoId_FechaInicio_FechaFin",
                table: "BloqueosDisponibilidad",
                columns: new[] { "VehiculoId", "FechaInicio", "FechaFin" });

            migrationBuilder.CreateIndex(
                name: "UX_Bloqueos_ReferenciaExternaId",
                table: "BloqueosDisponibilidad",
                column: "ReferenciaExternaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculos_Tipo",
                table: "Vehiculos",
                column: "Tipo");

            migrationBuilder.CreateIndex(
                name: "UX_Vehiculos_Placa",
                table: "Vehiculos",
                column: "Placa",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BloqueosDisponibilidad");

            migrationBuilder.DropTable(
                name: "Vehiculos");
        }
    }
}
