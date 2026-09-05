using FluentAssertions;
using FluentValidation;
using RentaFacil.Bookings.Application.Behaviors;
using RentaFacil.SharedKernel.Results;

namespace RentaFacil.Bookings.Application.UnitTests.Behaviors;

// ValidationBehavior construye el TResponse de fallo por reflexión (para soportar tanto
// Result como Result<T> genérico); estos tests usan comandos de prueba locales para
// ejercitar ambos caminos sin depender de detalles de un command real del dominio.
public class ValidationBehaviorTests
{
    private sealed record TestCommand(string Nombre);

    private sealed class TestCommandValidator : AbstractValidator<TestCommand>
    {
        public TestCommandValidator()
        {
            RuleFor(command => command.Nombre).NotEmpty();
        }
    }

    private sealed record TestVoidCommand(string Nombre);

    private sealed class TestVoidCommandValidator : AbstractValidator<TestVoidCommand>
    {
        public TestVoidCommandValidator()
        {
            RuleFor(command => command.Nombre).NotEmpty();
        }
    }

    [Fact]
    public async Task Handle_SinValidadoresRegistrados_LlamaSiguienteEnLaCadena()
    {
        var behavior = new ValidationBehavior<TestCommand, Result<string>>(Array.Empty<IValidator<TestCommand>>());

        var resultado = await behavior.Handle(
            new TestCommand("Juan"),
            _ => Task.FromResult(Result.Success("ok")),
            CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_ConValidacionExitosa_LlamaSiguienteEnLaCadena()
    {
        var behavior = new ValidationBehavior<TestCommand, Result<string>>([new TestCommandValidator()]);

        var resultado = await behavior.Handle(
            new TestCommand("Juan"),
            _ => Task.FromResult(Result.Success("ok")),
            CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_ConValidacionFallidaYRespuestaGenerica_RetornaResultTDeValidacionSinLlamarSiguiente()
    {
        var behavior = new ValidationBehavior<TestCommand, Result<string>>([new TestCommandValidator()]);
        var siguienteInvocado = false;

        var resultado = await behavior.Handle(
            new TestCommand(string.Empty),
            _ =>
            {
                siguienteInvocado = true;
                return Task.FromResult(Result.Success("no debería llegar aquí"));
            },
            CancellationToken.None);

        siguienteInvocado.Should().BeFalse();
        resultado.IsFailure.Should().BeTrue();
        resultado.Error.ErrorType.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Handle_ConValidacionFallidaYRespuestaNoGenerica_RetornaResultDeValidacionSinLlamarSiguiente()
    {
        var behavior = new ValidationBehavior<TestVoidCommand, Result>([new TestVoidCommandValidator()]);
        var siguienteInvocado = false;

        var resultado = await behavior.Handle(
            new TestVoidCommand(string.Empty),
            _ =>
            {
                siguienteInvocado = true;
                return Task.FromResult(Result.Success());
            },
            CancellationToken.None);

        siguienteInvocado.Should().BeFalse();
        resultado.IsFailure.Should().BeTrue();
        resultado.Error.ErrorType.Should().Be(ErrorType.Validation);
    }
}
