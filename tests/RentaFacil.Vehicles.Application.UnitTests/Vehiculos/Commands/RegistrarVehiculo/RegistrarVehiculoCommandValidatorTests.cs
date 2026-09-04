using FluentValidation.TestHelper;
using Moq;
using RentaFacil.SharedKernel.Abstractions;
using RentaFacil.Vehicles.Application.Vehiculos.Commands.RegistrarVehiculo;
using RentaFacil.Vehicles.Domain.Enums;

namespace RentaFacil.Vehicles.Application.UnitTests.Vehiculos.Commands.RegistrarVehiculo;

public class RegistrarVehiculoCommandValidatorTests
{
    private readonly RegistrarVehiculoCommandValidator _validator;

    public RegistrarVehiculoCommandValidatorTests()
    {
        var dateTimeProviderMock = new Mock<IDateTimeProvider>();
        dateTimeProviderMock.Setup(provider => provider.UtcNow).Returns(new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc));

        _validator = new RegistrarVehiculoCommandValidator(dateTimeProviderMock.Object);
    }

    [Fact]
    public void Validate_ConDatosValidos_NoTieneErrores()
    {
        var command = new RegistrarVehiculoCommand("ABC123", TipoVehiculo.SUV, "Toyota", "Fortuner", 2024, 100_000m, "COP");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ConPlacaVacia_TieneErrorEnPlaca()
    {
        var command = new RegistrarVehiculoCommand(string.Empty, TipoVehiculo.SUV, "Toyota", "Fortuner", 2024, 100_000m, "COP");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Placa);
    }

    [Fact]
    public void Validate_ConTarifaCero_TieneErrorEnTarifaDiaria()
    {
        var command = new RegistrarVehiculoCommand("ABC123", TipoVehiculo.SUV, "Toyota", "Fortuner", 2024, 0m, "COP");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.TarifaDiaria);
    }

    [Fact]
    public void Validate_ConAnioFueraDeRango_TieneErrorEnAnio()
    {
        var command = new RegistrarVehiculoCommand("ABC123", TipoVehiculo.SUV, "Toyota", "Fortuner", 1980, 100_000m, "COP");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Anio);
    }
}
