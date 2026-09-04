using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentaFacil.Bookings.Domain.Entities;
using RentaFacil.Bookings.Domain.ValueObjects;

namespace RentaFacil.Bookings.Infrastructure.Persistence.Configurations;

internal sealed class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    private const char Separador = ':';

    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");

        builder.HasKey(cliente => cliente.Id);

        builder.Property(cliente => cliente.Id)
            .ValueGeneratedNever();

        builder.Property(cliente => cliente.Documento)
            .HasConversion(
                documento => $"{documento.Tipo}{Separador}{documento.Numero}",
                valor => ParseDocumento(valor))
            .HasColumnName("Documento")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(cliente => cliente.Documento)
            .IsUnique()
            .HasDatabaseName("UX_Clientes_Documento");

        builder.Property(cliente => cliente.NombreCompleto)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(cliente => cliente.Email)
            .HasConversion(email => email.Valor, valor => Email.Create(valor).Value)
            .HasColumnName("Email")
            .HasMaxLength(254)
            .IsRequired();

        builder.Property(cliente => cliente.Telefono)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(cliente => cliente.FechaRegistro)
            .HasColumnType("datetime2")
            .IsRequired();
    }

    private static Documento ParseDocumento(string valor)
    {
        var partes = valor.Split(Separador, 2);
        return Documento.Create(partes[0], partes[1]).Value;
    }
}
