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

        builder.HasMany(vehiculo => vehiculo.Bloqueos)
            .WithOne()
            .HasForeignKey(bloqueo => bloqueo.VehiculoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(vehiculo => vehiculo.Bloqueos)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
