using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentaFacil.Bookings.Domain.Entities;

namespace RentaFacil.Bookings.Infrastructure.Persistence.Configurations;

internal sealed class ReservaConfiguration : IEntityTypeConfiguration<Reserva>
{
    public void Configure(EntityTypeBuilder<Reserva> builder)
    {
        builder.ToTable("Reservas");

        builder.HasKey(reserva => reserva.Id);

        builder.Property(reserva => reserva.Id)
            .ValueGeneratedNever();

        builder.Property(reserva => reserva.ClienteId)
            .IsRequired();

        // FK a Clientes con Restrict (§3.2): a diferencia de Vehiculo/BloqueoDisponibilidad
        // en Vehicles.Domain, Cliente no expone una colección de Reservas ni Reserva se quita
        // de ninguna colección en memoria, así que no aplica la limitación de EF Core que
        // obligó a usar ClientCascade en la Fase 2.
        builder.HasOne<Cliente>()
            .WithMany()
            .HasForeignKey(reserva => reserva.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        // VehiculoId es una referencia lógica a VehicleService, sin FK física (§11 decisión 9).
        builder.Property(reserva => reserva.VehiculoId)
            .IsRequired();

        builder.Property(reserva => reserva.TipoVehiculo)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(reserva => reserva.PlacaVehiculo)
            .HasMaxLength(10)
            .IsRequired();

        builder.OwnsOne(reserva => reserva.Periodo, periodo =>
        {
            periodo.Property(p => p.FechaInicio)
                .HasColumnName("FechaInicio")
                .HasColumnType("date")
                .IsRequired();

            periodo.Property(p => p.FechaFin)
                .HasColumnName("FechaFin")
                .HasColumnType("date")
                .IsRequired();
        });

        builder.OwnsOne(reserva => reserva.TarifaDiariaAplicada, tarifa =>
        {
            tarifa.Property(dinero => dinero.Monto)
                .HasColumnName("TarifaDiariaMonto")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            tarifa.Property(dinero => dinero.Moneda)
                .HasColumnName("TarifaDiariaMoneda")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.OwnsOne(reserva => reserva.ValorTotal, valorTotal =>
        {
            valorTotal.Property(dinero => dinero.Monto)
                .HasColumnName("ValorTotalMonto")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            valorTotal.Property(dinero => dinero.Moneda)
                .HasColumnName("ValorTotalMoneda")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Property(reserva => reserva.FechaCreacion)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.HasIndex(reserva => reserva.FechaCreacion)
            .HasDatabaseName("IX_Reservas_FechaCreacion");
    }
}
