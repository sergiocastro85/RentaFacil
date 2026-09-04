using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentaFacil.Vehicles.Domain.Entities;
using RentaFacil.Vehicles.Domain.ValueObjects;

namespace RentaFacil.Vehicles.Infrastructure.Persistence.Configurations;

internal sealed class VehiculoConfiguration : IEntityTypeConfiguration<Vehiculo>
{
    public void Configure(EntityTypeBuilder<Vehiculo> builder)
    {
        builder.ToTable("Vehiculos");

        builder.HasKey(vehiculo => vehiculo.Id);

        builder.Property(vehiculo => vehiculo.Id)
            .ValueGeneratedNever();

        builder.Property(vehiculo => vehiculo.Placa)
            .HasConversion(placa => placa.Valor, valor => Placa.Create(valor).Value)
            .HasMaxLength(10)
            .IsRequired();

        builder.HasIndex(vehiculo => vehiculo.Placa)
            .IsUnique()
            .HasDatabaseName("UX_Vehiculos_Placa");

        builder.Property(vehiculo => vehiculo.Tipo)
            .HasConversion<int>()
            .IsRequired();

        builder.HasIndex(vehiculo => vehiculo.Tipo)
            .HasDatabaseName("IX_Vehiculos_Tipo");

        builder.Property(vehiculo => vehiculo.Marca)
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(vehiculo => vehiculo.Modelo)
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(vehiculo => vehiculo.Anio)
            .IsRequired();

        builder.OwnsOne(vehiculo => vehiculo.TarifaDiaria, tarifa =>
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

        builder.Property(vehiculo => vehiculo.FechaRegistro)
            .HasColumnType("datetime2")
            .IsRequired();

        // ClientCascade (no Cascade ni Restrict): en la base de datos la FK sigue siendo
        // "ON DELETE NO ACTION" -- el mismo DDL que generaría Restrict en SQL Server, así
        // que se respeta "OnDelete: Restrict" de ARCHITECTURE.md §3.1 -- pero el change
        // tracker de EF Core sí borra la fila huérfana cuando Vehiculo.LiberarBloqueo() la
        // quita de la colección en memoria. Con Restrict a secas, EF lanza
        // InvalidOperationException porque la FK es obligatoria y no sabe qué hacer con un
        // hijo que perdió su padre sin que la base tenga permiso de borrar en cascada.
        builder.HasMany(vehiculo => vehiculo.Bloqueos)
            .WithOne()
            .HasForeignKey(bloqueo => bloqueo.VehiculoId)
            .OnDelete(DeleteBehavior.ClientCascade);

        builder.Navigation(vehiculo => vehiculo.Bloqueos)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
