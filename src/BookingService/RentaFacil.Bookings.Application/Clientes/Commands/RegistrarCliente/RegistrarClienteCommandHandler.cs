using MediatR;
using RentaFacil.SharedKernel.Abstractions;
using RentaFacil.SharedKernel.Results;
using RentaFacil.Bookings.Application.DTOs;
using RentaFacil.Bookings.Domain.Entities;
using RentaFacil.Bookings.Domain.Errors;
using RentaFacil.Bookings.Domain.Repositories;
using RentaFacil.Bookings.Domain.ValueObjects;

namespace RentaFacil.Bookings.Application.Clientes.Commands.RegistrarCliente;

public sealed class RegistrarClienteCommandHandler : IRequestHandler<RegistrarClienteCommand, Result<ClienteResponse>>
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RegistrarClienteCommandHandler(
        IClienteRepository clienteRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _clienteRepository = clienteRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<ClienteResponse>> Handle(
        RegistrarClienteCommand request,
        CancellationToken cancellationToken)
    {
        var documentoResult = Documento.Create(request.TipoDocumento, request.NumeroDocumento);
        if (documentoResult.IsFailure)
        {
            return Result.Failure<ClienteResponse>(documentoResult.Error);
        }

        var clienteExistente = await _clienteRepository.ObtenerPorDocumentoAsync(documentoResult.Value, cancellationToken);
        if (clienteExistente is not null)
        {
            return Result.Failure<ClienteResponse>(ClienteErrors.DocumentoDuplicado(documentoResult.Value.Numero));
        }

        var fechaActual = _dateTimeProvider.UtcNow;

        var clienteResult = Cliente.Crear(
            Guid.NewGuid(),
            request.TipoDocumento,
            request.NumeroDocumento,
            request.NombreCompleto,
            request.Email,
            request.Telefono,
            fechaActual);

        if (clienteResult.IsFailure)
        {
            return Result.Failure<ClienteResponse>(clienteResult.Error);
        }

        var cliente = clienteResult.Value;

        await _clienteRepository.AgregarAsync(cliente, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ClienteResponse(
            cliente.Id,
            $"{cliente.Documento.Tipo}:{cliente.Documento.Numero}",
            cliente.NombreCompleto,
            cliente.Email.Valor,
            cliente.Telefono);
    }
}
