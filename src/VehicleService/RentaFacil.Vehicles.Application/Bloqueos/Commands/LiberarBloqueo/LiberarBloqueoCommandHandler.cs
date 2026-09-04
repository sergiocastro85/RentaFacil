using MediatR;
using RentaFacil.SharedKernel.Abstractions;
using RentaFacil.SharedKernel.Results;
using RentaFacil.Vehicles.Domain.Errors;
using RentaFacil.Vehicles.Domain.Repositories;

namespace RentaFacil.Vehicles.Application.Bloqueos.Commands.LiberarBloqueo;

public sealed class LiberarBloqueoCommandHandler : IRequestHandler<LiberarBloqueoCommand, Result>
{
    private readonly IVehiculoRepository _vehiculoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LiberarBloqueoCommandHandler(IVehiculoRepository vehiculoRepository, IUnitOfWork unitOfWork)
    {
        _vehiculoRepository = vehiculoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(LiberarBloqueoCommand request, CancellationToken cancellationToken)
    {
        var vehiculo = await _vehiculoRepository.ObtenerPorIdAsync(request.VehiculoId, cancellationToken);
        if (vehiculo is null)
        {
            return Result.Failure(VehiculoErrors.VehiculoNoEncontrado(request.VehiculoId));
        }

        var bloqueoExiste = vehiculo.Bloqueos.Any(bloqueo => bloqueo.ReferenciaExternaId == request.ReferenciaExternaId);
        if (!bloqueoExiste)
        {
            return Result.Failure(VehiculoErrors.BloqueoNoEncontrado(request.ReferenciaExternaId));
        }

        vehiculo.LiberarBloqueo(request.ReferenciaExternaId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
