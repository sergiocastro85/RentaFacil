namespace RentaFacil.Reporting.Worker;

public sealed class ReportingWorkerOptions
{
    public const string SectionName = "ReportingWorker";

    public string CronExpression { get; init; } = "55 23 * * *";

    public bool EjecutarAlIniciar { get; init; }
}
