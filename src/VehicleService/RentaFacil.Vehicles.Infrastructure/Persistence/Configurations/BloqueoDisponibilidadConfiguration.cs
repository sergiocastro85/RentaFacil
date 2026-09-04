using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentaFacil.Vehicles.Domain.Entities;

namespace RentaFacil.Vehicles.Infrastructure.Persistence.Configurations;

internal sealed class BloqueoDisponibilidadConfiguration : IEntityTypeConfiguration<BloqueoDisponibilidad>
{
    public void Configure(EntityTypeBuilder<BloqueoDisponibilidad> builder)
    {
        builder.ToTable("BloqueosDisponibilidad");

        builder.HasKey(bloqueo => bloqueo.Id);

        builder.Property(bloqueo => bloqueo.Id)
            .ValueGeneratedNever();

        builder.Property(bloqueo => bloqueo.VehiculoId)
            .IsRequired();

        builder.OwnsOne(bloqueo => bloqueo.Periodo, periodo =>
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

        builder.Property(bloqueo => bloqueo.ReferenciaExternaId)
            .IsRequired();

        builder.Property(bloqueo => bloqueo.FechaCreacion)
            .HasColumnType("datetime2")
            .IsRequired();

        // El índice compuesto IX_Bloqueos_VehiculoId_FechaInicio_FechaFin se agrega manualmente
        // en la migración: EF Core no soporta índices que combinen una propiedad del propietario
        // con propiedades de un owned type (dotnet/efcore#11336).

        builder.HasIndex(bloqueo => bloqueo.ReferenciaExternaId)
            .IsUnique()
            .HasDatabaseName("UX_Bloqueos_ReferenciaExternaId");
    }
}
