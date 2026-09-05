using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RentaFacil.Bookings.Application.Behaviors;
using RentaFacil.SharedKernel.Results;

namespace RentaFacil.Bookings.Application.UnitTests.Behaviors;

public class LoggingBehaviorTests
{
    public sealed record TestCommand(string Nombre);

    private readonly Mock<ILogger<LoggingBehavior<TestCommand, Result<string>>>> _loggerMock = new();

    [Fact]
    public async Task Handle_CuandoSeInvoca_LlamaSiguienteEnLaCadenaYRetornaSuResultado()
    {
        var behavior = new LoggingBehavior<TestCommand, Result<string>>(_loggerMock.Object);
        var siguienteInvocado = false;

        var resultado = await behavior.Handle(
            new TestCommand("Juan"),
            _ =>
            {
                siguienteInvocado = true;
                return Task.FromResult(Result.Success("ok"));
            },
            CancellationToken.None);

        siguienteInvocado.Should().BeTrue();
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_ConResultadoExitoso_LoguearInicioYFinConLogInformation()
    {
        var behavior = new LoggingBehavior<TestCommand, Result<string>>(_loggerMock.Object);

        await behavior.Handle(
            new TestCommand("Juan"),
            _ => Task.FromResult(Result.Success("ok")),
            CancellationToken.None);

        VerifyLog(LogLevel.Information, Times.Exactly(2));
        VerifyLog(LogLevel.Warning, Times.Never());
    }

    [Fact]
    public async Task Handle_ConResultadoFallido_LoguearFinConLogWarningDistintoDeUnoExitoso()
    {
        var behavior = new LoggingBehavior<TestCommand, Result<string>>(_loggerMock.Object);
        var error = new Error("Test.Error", "Error de prueba", ErrorType.Validation);

        var resultado = await behavior.Handle(
            new TestCommand("Juan"),
            _ => Task.FromResult(Result.Failure<string>(error)),
            CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        VerifyLog(LogLevel.Information, Times.Once());
        VerifyLog(LogLevel.Warning, Times.Once());
    }

    private void VerifyLog(LogLevel level, Times times)
    {
        _loggerMock.Verify(
            logger => logger.Log(
                level,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }
}
