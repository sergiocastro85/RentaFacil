using MediatR;
using RentaFacil.SharedKernel.Results;
using RentaFacil.Bookings.Application.DTOs;

namespace RentaFacil.Bookings.Application.Reportes.Commands.GenerarReporteDiario;

public sealed record GenerarReporteDiarioCommand(DateOnly Fecha) : IRequest<Result<ReporteDiarioDto>>;
