namespace RentaFacil.SharedKernel.Abstractions;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Ejecuta <paramref name="operation"/> dentro de una transacción atómica. El nivel de
    /// aislamiento y la estrategia de reintento son responsabilidad de cada implementación
    /// concreta de Infrastructure, según lo que necesite su contexto.
    /// </summary>
    Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken);
}
