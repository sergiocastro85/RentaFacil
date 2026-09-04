using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentaFacil.Vehicles.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BloqueoDeleteBehaviorClientCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Cambia el DeleteBehavior de Restrict a ClientCascade (ver VehiculoConfiguration).
            // No hay diferencia de comportamiento en la base de datos: SQL Server no tiene
            // una acción "RESTRICT" propia, así que tanto Restrict como la ausencia de
            // "onDelete" (el caso de ClientCascade) generan "ON DELETE NO ACTION". El único
            // cambio real es que el change tracker de EF Core ahora sí borra el
            // BloqueoDisponibilidad huérfano cuando Vehiculo.LiberarBloqueo() lo quita de la
            // colección en memoria, en lugar de lanzar InvalidOperationException.
            migrationBuilder.DropForeignKey(
                name: "FK_BloqueosDisponibilidad_Vehiculos_VehiculoId",
                table: "BloqueosDisponibilidad");

            migrationBuilder.AddForeignKey(
                name: "FK_BloqueosDisponibilidad_Vehiculos_VehiculoId",
                table: "BloqueosDisponibilidad",
                column: "VehiculoId",
                principalTable: "Vehiculos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BloqueosDisponibilidad_Vehiculos_VehiculoId",
                table: "BloqueosDisponibilidad");

            migrationBuilder.AddForeignKey(
                name: "FK_BloqueosDisponibilidad_Vehiculos_VehiculoId",
                table: "BloqueosDisponibilidad",
                column: "VehiculoId",
                principalTable: "Vehiculos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
