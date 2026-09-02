# CLAUDE.md — RentaFácil S.A.S.

Reglas operativas para trabajar en este repositorio.
El diseño completo está en `ARCHITECTURE.md`. **Léelo antes de escribir código** y no
contradigas ninguna decisión de ese documento sin avisarme primero.

---

## Regla de alcance (la más importante)

**No agregues nada que no esté en `ARCHITECTURE.md`.**

Nada de endpoints "útiles" extra, entidades de conveniencia, campos previsores,
health checks, paginación, autenticación, cancelación de reservas, ni interfaces
"por si acaso". Si crees que falta algo, **dilo y espera confirmación**; no lo
implementes por tu cuenta.

Si un requisito del documento es ambiguo, pregunta. No inventes la interpretación.

---

## Stack

- .NET 8, C# 12, `Nullable` habilitado, `TreatWarningsAsErrors`
- **La máquina tiene instalado el SDK de .NET 10, pero el TFM del proyecto es
  `net8.0`.** Pasa siempre `-f net8.0` en `dotnet new` e instala los paquetes NuGet
  de Microsoft en versión `8.x`. Ningún `.csproj` puede quedar en `net10.0`.
- EF Core 8 + SQL Server 2022 (contenedor)
- MediatR, FluentValidation, Serilog, Polly
- xUnit + Moq + FluentAssertions + coverlet

---

## Convenciones de código

- Namespaces: `RentaFacil.<Contexto>.<Capa>` (ej. `RentaFacil.Vehicles.Domain`)
- Nombres de dominio en **español** (`Vehiculo`, `Reserva`, `Cliente`, `Periodo`),
  nombres de infraestructura y patrones en **inglés** (`Repository`, `Handler`,
  `DbContext`, `Middleware`). No mezcles dentro de un mismo identificador.
- Controladores: sufijo `Controller`, heredan de `ControllerBase`, **delgados**:
  request → command/query → `ISender.Send()` → traducir `Result` a `IActionResult`.
  Cero `if` de negocio, cero `try/catch`.
- Servicios: interfaz `IAlgo` + implementación `Algo`.
- DTOs siempre separados de las entidades. Una entidad de dominio **nunca** cruza
  la frontera HTTP.
- `async/await` en todo I/O. Sufijo `Async`. Siempre propaga `CancellationToken`.
- Errores de negocio → `Result<T>` con un `Error` tipado. Excepciones **solo** para
  fallos reales e inesperados.
- Logging estructurado con Serilog. **Nunca** `Console.WriteLine`, nunca
  interpolación de strings en el mensaje de log (usa plantillas con placeholders).
- Un archivo por tipo público.

## Dominio

- `Domain` no referencia EF Core, MediatR ni ASP.NET. Solo `SharedKernel`.
- Constructores privados + método estático `Crear()` que devuelve `Result<T>` y
  valida invariantes. Nadie construye una entidad en estado inválido.
- Value objects inmutables, con igualdad estructural.
- Sin setters públicos: la mutación va por métodos con nombre de negocio.

## Persistencia

- Configuración con **Fluent API** en clases `IEntityTypeConfiguration<T>`, una por
  entidad, en `Infrastructure/Persistence/Configurations`. Cero Data Annotations.
- `Periodo` y `Dinero` como owned types (`OwnsOne`).
- Enums como `int` con conversión explícita.
- Todo monto `decimal(18,2)`. Todo `DateTime` en UTC, columna `datetime2`.
- Queries de lectura con `AsNoTracking()` y proyección directa al DTO
  (nada de traer la entidad completa para mapearla en memoria).
- El `DbContext` **no sale** de `Infrastructure`.
- Índices: los definidos en `ARCHITECTURE.md` §3, ni uno más.
- Cuidado con N+1: si una query recorre una colección, revísala.

## Pruebas

- Solo pruebas unitarias (así está definido el alcance).
- Nombres: `Metodo_Escenario_ResultadoEsperado`.
- Un test por caso: éxito, error de validación, error de negocio.
- Los handlers se testean con repositorios y puertos **mockeados**, nunca con base
  de datos real. Excepción: la agregación del Worker usa EF InMemory.
- No escribas tests triviales de getters para inflar cobertura.

---

## Flujo de trabajo

Trabajamos **por fases**, según el plan de `ARCHITECTURE.md` §12.
En cada fase:

1. Confirma qué fase vamos a hacer y qué archivos vas a crear. Espera mi visto bueno.
2. Implementa **solo** esa fase.
3. Ejecuta `dotnet build` y `dotnet test`. Si algo falla, arréglalo antes de reportar.
4. Muéstrame un resumen de lo hecho y el mensaje de commit propuesto.
5. **No hagas commit tú.** Yo commiteo.

No adelantes fases. No refactorices código de fases anteriores sin decírmelo.

## Commits (Conventional Commits, en español)

```
feat(vehicles): registrar vehículo con validación de placa única
feat(bookings): crear reserva con bloqueo de cupo en VehicleService
test(vehicles): pruebas de solapamiento de periodos
chore: estructura inicial de la solución
docs: instrucciones de instalación en el README
```

Ámbitos válidos: `vehicles`, `bookings`, `reporting`, `shared`, `infra`.

---

## Comandos frecuentes

```bash
dotnet build
dotnet test
dotnet test --collect:"XPlat Code Coverage"

docker compose up -d sqlserver
docker compose up --build

# migraciones (ejemplo VehicleService)
dotnet ef migrations add <Nombre> \
  -p src/VehicleService/RentaFacil.Vehicles.Infrastructure \
  -s src/VehicleService/RentaFacil.Vehicles.Api \
  -o Persistence/Migrations
```

---

## Qué NO hacer

- No generes código de fases futuras.
- No agregues paquetes NuGet fuera del stack listado sin preguntar.
- No cambies la estructura de carpetas definida en `ARCHITECTURE.md` §1.
- No pongas lógica de negocio en controladores, repositorios ni en el Worker.
- No uses `DateTime.Now`: usa `IDateTimeProvider` (UTC).
- No expongas entidades de EF en las respuestas HTTP.
- No crees un proyecto `Common`/`Utils` de cajón de sastre.
