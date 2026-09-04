using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentaFacil.Bookings.Domain.Entities;

namespace RentaFacil.Bookings.Infrastructure.Persistence.Configurations;

internal sealed class ReporteReservasDiariasConfiguration : IEntityTypeConfiguration<ReporteReservasDiarias>
{
    public void Configure(EntityTypeBuilder<ReporteReservasDiarias> builder)
    {
        builder.ToTable("ReporteReservasDiarias");

        builder.HasKey(reporte => reporte.Id);

        builder.Property(reporte => reporte.Id)
            .ValueGeneratedNever();

        builder.Property(reporte => reporte.Fecha)
            .HasColumnType("date")
            .IsRequired();

        builder.HasIndex(reporte => reporte.Fecha)
            .IsUnique()
            .HasDatabaseName("UX_ReporteReservasDiarias_Fecha");

        builder.Property(reporte => reporte.TotalReservas)
            .IsRequired();

        builder.Property(reporte => reporte.ValorTotalReservado)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(reporte => reporte.TipoVehiculoMasReservado)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(reporte => reporte.ClientesUnicos)
            .IsRequired();

        builder.Property(reporte => reporte.DetalleJson)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(reporte => reporte.FechaProcesamiento)
            .HasColumnType("datetime2")
            .IsRequired();
    }
}
