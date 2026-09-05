using FluentAssertions;
using Moq;
using RentaFacil.SharedKernel.Abstractions;
using RentaFacil.Bookings.Application.Clientes.Commands.RegistrarCliente;
using RentaFacil.Bookings.Domain.Entities;
using RentaFacil.Bookings.Domain.Repositories;
using RentaFacil.Bookings.Domain.ValueObjects;

namespace RentaFacil.Bookings.Application.UnitTests.Clientes.Commands.RegistrarCliente;

public class RegistrarClienteCommandHandlerTests
{
    private static readonly DateTime FechaActual = new(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IClienteRepository> _clienteRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
    private readonly RegistrarClienteCommandHandler _handler;

    public RegistrarClienteCommandHandlerTests()
    {
        _dateTimeProviderMock.Setup(provider => provider.UtcNow).Returns(FechaActual);
        _handler = new RegistrarClienteCommandHandler(
            _clienteRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _dateTimeProviderMock.Object);
    }

    [Fact]
    public async Task Handle_ConDatosValidos_RetornaExito()
    {
        _clienteRepositoryMock
            .Setup(repository => repository.ObtenerPorDocumentoAsync(It.IsAny<Documento>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        var command = new RegistrarClienteCommand(
            "CC", "1234567890", "Juan Pérez", "juan.perez@rentafacil.com", "3001234567");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.NombreCompleto.Should().Be("Juan Pérez");
        _clienteRepositoryMock.Verify(
            repository => repository.AgregarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConDocumentoDuplicado_RetornaConflict()
    {
        var clienteExistente = Cliente.Crear(
            Guid.NewGuid(), "CC", "1234567890", "Juan Pérez", "juan.perez@rentafacil.com", "3001234567", FechaActual).Value;

        _clienteRepositoryMock
            .Setup(repository => repository.ObtenerPorDocumentoAsync(It.IsAny<Documento>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(clienteExistente);

        var command = new RegistrarClienteCommand(
            "CC", "1234567890", "Juan Pérez", "juan.perez@rentafacil.com", "3001234567");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cliente.DocumentoDuplicado");
        _clienteRepositoryMock.Verify(
            repository => repository.AgregarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ConDatosInvalidos_RetornaFalloDeDominio()
    {
        _clienteRepositoryMock
            .Setup(repository => repository.ObtenerPorDocumentoAsync(It.IsAny<Documento>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        var command = new RegistrarClienteCommand(
            "CC", "1234567890", "Juan Pérez", "correo-invalido", "3001234567");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cliente.EmailInvalido");
        _clienteRepositoryMock.Verify(
            repository => repository.AgregarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ConDocumentoInvalido_RetornaFalloDeDominioSinConsultarRepositorio()
    {
        var command = new RegistrarClienteCommand(
            "CC", "123ABC789", "Juan Pérez", "juan.perez@rentafacil.com", "3001234567");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cliente.DocumentoInvalido");
        _clienteRepositoryMock.Verify(
            repository => repository.ObtenerPorDocumentoAsync(It.IsAny<Documento>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
