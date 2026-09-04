# RentaFácil S.A.S. — Arquitectura del Backend

> Documento de diseño previo a la implementación.
> Alcance: **solo backend**. Sin Angular, sin Azure.
> Stack: .NET 8, Clean Architecture + DDD táctico, EF Core, SQL Server, Docker Compose.
>
> **Regla de alcance:** este documento cubre únicamente lo que exige el enunciado.
> Nada adicional. Cualquier funcionalidad nueva se agrega solo por indicación explícita.

---

## 0. Resumen ejecutivo

Tres ejecutables organizados en **dos bounded contexts**:

| Contexto | Ejecutable | Responsabilidad |
|---|---|---|
| Fleet | `VehicleService.Api` | Registro de vehículos y consulta de disponibilidad |
| Booking | `BookingService.Api` | Clientes, creación de reservas e historial |
| Booking | `ReportingWorker` | Consolidación diaria de reservas en tabla de reportes |

Cada contexto tiene **su propia base de datos** (misma instancia de SQL Server, bases separadas). No hay joins entre contextos: la comunicación es explícita vía HTTP.

Decisión clave: **la disponibilidad vive en VehicleService**. Ver §7.

---

## 1. Estructura de la solución y proyectos

```
RentaFacil.sln
│
├── docker-compose.yml
├── README.md
├── ARCHITECTURE.md                     ← este documento
├── .editorconfig
├── Directory.Build.props               ← LangVersion, Nullable, TreatWarningsAsErrors
│
├── src/
│   ├── Shared/
│   │   └── RentaFacil.SharedKernel/                    [classlib, sin dependencias]
│   │       ├── Primitives/         (Entity, AggregateRoot, ValueObject)
│   │       ├── Results/            (Result, Result<T>, Error, ErrorType)
│   │       └── Abstractions/       (IUnitOfWork, IDateTimeProvider)
│   │
│   ├── VehicleService/
│   │   ├── RentaFacil.Vehicles.Domain/
│   │   │   ├── Entities/           (Vehiculo, BloqueoDisponibilidad)
│   │   │   ├── ValueObjects/       (Placa, Periodo, Dinero)
│   │   │   ├── Enums/              (TipoVehiculo)
│   │   │   ├── Errors/             (VehiculoErrors)
│   │   │   └── Repositories/       (IVehiculoRepository)
│   │   │
│   │   ├── RentaFacil.Vehicles.Application/
│   │   │   ├── Vehiculos/
│   │   │   │   ├── Commands/RegistrarVehiculo/     (Command, Handler, Validator)
│   │   │   │   └── Queries/ConsultarDisponibilidad/
│   │   │   ├── Bloqueos/
│   │   │   │   ├── Commands/CrearBloqueo/
│   │   │   │   └── Commands/LiberarBloqueo/        (solo compensación, ver §7)
│   │   │   ├── Behaviors/          (ValidationBehavior, LoggingBehavior)
│   │   │   ├── DTOs/
│   │   │   └── DependencyInjection.cs
│   │   │
│   │   ├── RentaFacil.Vehicles.Infrastructure/
│   │   │   ├── Persistence/
│   │   │   │   ├── VehiclesDbContext.cs
│   │   │   │   ├── Configurations/  (IEntityTypeConfiguration<T> por entidad)
│   │   │   │   ├── Repositories/
│   │   │   │   └── Migrations/
│   │   │   └── DependencyInjection.cs
│   │   │
│   │   └── RentaFacil.Vehicles.Api/
│   │       ├── Controllers/         (VehiculosController)
│   │       ├── Middleware/          (ExceptionHandlingMiddleware)
│   │       ├── Extensions/          (SwaggerSetup, SerilogSetup, ResultToActionResult)
│   │       ├── Program.cs
│   │       ├── appsettings.json
│   │       └── Dockerfile
│   │
│   ├── BookingService/
│   │   ├── RentaFacil.Bookings.Domain/
│   │   │   ├── Entities/           (Cliente, Reserva, ReporteReservasDiarias)
│   │   │   ├── ValueObjects/       (Periodo, Dinero, Documento, Email)
│   │   │   ├── Errors/             (ReservaErrors, ClienteErrors)
│   │   │   └── Repositories/       (IReservaRepository, IClienteRepository, IReporteRepository)
│   │   │
│   │   ├── RentaFacil.Bookings.Application/
│   │   │   ├── Reservas/
│   │   │   │   ├── Commands/CrearReserva/
│   │   │   │   └── Queries/ObtenerHistorialPorCliente/
│   │   │   ├── Clientes/
│   │   │   │   └── Commands/RegistrarCliente/
│   │   │   ├── Reportes/
│   │   │   │   └── Commands/GenerarReporteDiario/    ← lo invoca el Worker
│   │   │   ├── Abstractions/       (IVehicleCatalogService  ← puerto hacia VehicleService)
│   │   │   ├── Behaviors/
│   │   │   ├── DTOs/
│   │   │   └── DependencyInjection.cs
│   │   │
│   │   ├── RentaFacil.Bookings.Infrastructure/
│   │   │   ├── Persistence/        (BookingsDbContext, Configurations, Repositories, Migrations)
│   │   │   ├── Http/               (VehicleCatalogHttpClient  ← adapter)
│   │   │   └── DependencyInjection.cs
│   │   │
│   │   ├── RentaFacil.Bookings.Api/
│   │   │   ├── Controllers/         (ClientesController, ReservasController)
│   │   │   ├── Middleware/
│   │   │   ├── Program.cs
│   │   │   └── Dockerfile
│   │   │
│   │   └── RentaFacil.Reporting.Worker/
│   │       ├── Workers/            (ReporteDiarioBackgroundService)
│   │       ├── Program.cs
│   │       └── Dockerfile
│
└── tests/
    ├── RentaFacil.Vehicles.Domain.UnitTests/
    ├── RentaFacil.Vehicles.Application.UnitTests/
    ├── RentaFacil.Bookings.Domain.UnitTests/
    ├── RentaFacil.Bookings.Application.UnitTests/
    └── RentaFacil.Reporting.UnitTests/
```

### Regla de dependencias

```
Api ──► Application ──► Domain ──► SharedKernel
 │                          ▲
 └──► Infrastructure ───────┘
```

- `Domain` no referencia nada salvo `SharedKernel`. Sin EF Core, sin MediatR, sin ASP.NET.
- `Application` referencia `Domain`. Define **interfaces** (puertos); no conoce SQL Server ni HttpClient.
- `Infrastructure` implementa los puertos de `Application`.
- `Api` solo compone: DI, middleware, controladores delgados.
- `Vehicles.*` y `Bookings.*` **nunca** se referencian entre sí a nivel de proyecto. Solo HTTP.

---

## 2. Responsabilidad de cada capa

| Capa | Contiene | NO contiene |
|---|---|---|
| **SharedKernel** | Primitivas (`Entity`, `ValueObject`, `AggregateRoot`), `Result<T>`, `Error`, `IDateTimeProvider` | Lógica de negocio de ningún contexto |
| **Domain** | Entidades, agregados, value objects, invariantes, interfaces de repositorio, errores de dominio | Acceso a datos, DTOs de API, atributos de EF |
| **Application** | Casos de uso (commands/queries con MediatR), validadores FluentValidation, DTOs, puertos hacia servicios externos | SQL, HTTP, detalles de framework web |
| **Infrastructure** | `DbContext`, configuraciones Fluent API, repositorios, migraciones, `HttpClient` tipado, Serilog | Reglas de negocio |
| **Api** | Controladores, middleware de errores, Swagger, composición de DI | Lógica de negocio, acceso directo a `DbContext` |
| **Worker** | `BackgroundService` y scheduling | Lógica de agregación (vive en `Application/Reportes`) |

**Regla dura:** un controlador nunca contiene `if` de negocio. Recibe request → mapea a command → `ISender.Send()` → traduce `Result` a `IActionResult`.

---

## 3. Entidades y relaciones

### 3.1 Contexto Fleet — base `RentaFacil_Vehicles`

**`Vehiculo`** (agregado raíz)

| Campo | Tipo | Notas |
|---|---|---|
| `Id` | `Guid` | PK, generado en dominio |
| `Placa` | `Placa` (VO) | único, normalizado a mayúsculas |
| `Tipo` | `TipoVehiculo` (enum) | `Sedan`, `SUV`, `Camioneta`, `Van`, `Pickup` |
| `Marca` / `Modelo` | `string(60)` | requeridos |
| `Anio` | `int` | 1990 ≤ año ≤ año actual + 1 |
| `TarifaDiaria` | `Dinero` (VO) | `decimal(18,2)` + moneda, > 0 |
| `FechaRegistro` | `DateTime` (UTC) | |

**`BloqueoDisponibilidad`** (entidad hija del agregado `Vehiculo`)

| Campo | Tipo | Notas |
|---|---|---|
| `Id` | `Guid` | PK |
| `VehiculoId` | `Guid` | FK → `Vehiculos`, `DeleteBehavior.ClientCascade` (DDL: `NO ACTION`; el borrado del hijo lo gobierna el agregado)|
| `Periodo` | `Periodo` (VO) | owned type → columnas `FechaInicio`, `FechaFin` (`date`) |
| `ReferenciaExternaId` | `Guid` | id de la reserva en el contexto Booking |
| `FechaCreacion` | `DateTime` (UTC) | |

Relación: `Vehiculo 1 ──── * BloqueoDisponibilidad`.

Índices:
- `UX_Vehiculos_Placa` (único).
- `IX_Vehiculos_Tipo` (filtro de la consulta de disponibilidad).
- `IX_Bloqueos_VehiculoId_FechaInicio_FechaFin` (detección de solapamiento).
- `UX_Bloqueos_ReferenciaExternaId` (único) → idempotencia: reintentar la misma reserva no crea dos bloqueos.

### 3.2 Contexto Booking — base `RentaFacil_Bookings`

**`Cliente`** (agregado raíz)

| Campo | Tipo | Notas |
|---|---|---|
| `Id` | `Guid` | PK |
| `Documento` | `Documento` (VO) | tipo + número, único |
| `NombreCompleto` | `string(150)` | |
| `Email` | `Email` (VO) | formato validado |
| `Telefono` | `string(20)` | |
| `FechaRegistro` | `DateTime` (UTC) | |

**`Reserva`** (agregado raíz)

| Campo | Tipo | Notas |
|---|---|---|
| `Id` | `Guid` | PK |
| `ClienteId` | `Guid` | FK → `Clientes`, `OnDelete: Restrict` |
| `VehiculoId` | `Guid` | **referencia lógica** al otro contexto, sin FK física |
| `TipoVehiculo` | `TipoVehiculo` (enum) | copia denormalizada para el historial y el reporte; persistido como `int` |
| `PlacaVehiculo` | `string(10)` | snapshot al momento de reservar |
| `Periodo` | `Periodo` (VO) | owned type |
| `TarifaDiariaAplicada` | `Dinero` (VO) | snapshot: la tarifa puede cambiar después |
| `ValorTotal` | `Dinero` (VO) | calculado en dominio |
| `FechaCreacion` | `DateTime` (UTC) | indexado — lo consume el Worker |

**`ReporteReservasDiarias`** (tabla que escribe el Worker)

| Campo | Tipo | Notas |
|---|---|---|
| `Id` | `Guid` | PK |
| `Fecha` | `date` | **único** → idempotencia del Worker |
| `TotalReservas` | `int` | |
| `ValorTotalReservado` | `decimal(18,2)` | |
| `TipoVehiculoMasReservado` | `string(30)` | |
| `ClientesUnicos` | `int` | |
| `DetalleJson` | `nvarchar(max)` | desglose por tipo de vehículo |
| `FechaProcesamiento` | `DateTime` (UTC) | |

Índices: `UX_ReporteReservasDiarias_Fecha`, `IX_Reservas_ClienteId`, `IX_Reservas_FechaCreacion`.

### 3.3 Value Objects

- `Periodo(FechaInicio, FechaFin)` — invariante `FechaFin > FechaInicio`. Método `SeSolapaCon(Periodo otro)` con la regla `inicioA < finB && inicioB < finA` (extremos abiertos: una reserva que termina el día 10 permite otra que inicia el 10).
- `Dinero(Monto, Moneda)` — no permite mezclar monedas ni montos negativos.
- `Placa`, `Documento`, `Email` — constructor privado + `Create()` que devuelve `Result<T>`.

> Se duplican deliberadamente en cada contexto en lugar de moverlos al `SharedKernel`: son conceptos distintos que hoy coinciden. Compartirlos acoplaría los bounded contexts.

---

## 4. DTOs

**Convención:** `*Request` (entrada API) → `*Command`/`*Query` (Application) → `*Response`/`*Dto` (salida). Las entidades de dominio nunca cruzan la frontera HTTP.
> Los endpoints bindean directamente al `Command`/`Query` cuando el contrato HTTP
> coincide 1:1 con él: el command **es** el DTO de entrada entre `Api` y `Application`,
> y una clase `Request` intermedia sería una copia sin valor. Solo se define un
> `*Request` propio cuando el body no coincide con el command, como en
> `CrearBloqueoRequest`, donde `vehiculoId` viene de la ruta.

### VehicleService

```
RegistrarVehiculoRequest      { placa, tipo, marca, modelo, anio, tarifaDiaria, moneda }
VehiculoResponse              { id, placa, tipo, marca, modelo, anio, tarifaDiaria }
ConsultaDisponibilidadRequest { tipo, fechaInicio, fechaFin }          ← query string
VehiculoDisponibleDto         { id, placa, tipo, marca, modelo, tarifaDiaria }
CrearBloqueoRequest           { fechaInicio, fechaFin, referenciaExternaId }
BloqueoResponse               { bloqueoId, vehiculoId, placa, tipo, tarifaDiaria, fechaInicio, fechaFin }
```

### BookingService

```
RegistrarClienteRequest   { tipoDocumento, numeroDocumento, nombreCompleto, email, telefono }
ClienteResponse           { id, documento, nombreCompleto, email, telefono }
CrearReservaRequest       { clienteId, vehiculoId, fechaInicio, fechaFin }
ReservaResponse           { id, clienteId, vehiculoId, placaVehiculo, fechaInicio, fechaFin, valorTotal }
HistorialReservaDto       { id, vehiculoId, placaVehiculo, tipoVehiculo, fechaInicio, fechaFin, valorTotal, fechaCreacion }
```

### Contrato de errores

Todos los errores salen como **RFC 7807 `ProblemDetails`** desde el middleware global:

```json
{
  "type": "https://rentafacil/errors/vehiculo-no-disponible",
  "title": "Vehículo no disponible",
  "status": 409,
  "detail": "El vehículo ABC123 ya tiene una reserva entre 2026-09-10 y 2026-09-15.",
  "traceId": "00-4bf92f...-01"
}
```

---

## 5. Endpoints

### VehicleService — `http://localhost:5001`

| Método | Ruta | Descripción | Códigos |
|---|---|---|---|
| `POST` | `/api/vehiculos` | Registrar vehículo | 201, 400, 409 (placa duplicada) |
| `GET` | `/api/vehiculos/disponibilidad?tipo=SUV&fechaInicio=…&fechaFin=…` | Disponibilidad por tipo y rango de fechas | 200, 400 |
| `POST` | `/api/vehiculos/{id}/bloqueos` | Reservar el cupo — consumido por BookingService | 201, 404, **409 conflicto** |
| `DELETE` | `/api/vehiculos/{id}/bloqueos/{referenciaExternaId}` | Liberar cupo — solo compensación (§7) | 204, 404 |

### BookingService — `http://localhost:5002`

| Método | Ruta | Descripción | Códigos |
|---|---|---|---|
| `POST` | `/api/clientes` | Registrar cliente | 201, 400, 409 |
| `POST` | `/api/reservas` | Crear reserva asociando cliente y vehículo | 201, 400, 404, 409, 503 |
| `GET` | `/api/clientes/{clienteId}/reservas` | Historial de reservas del cliente | 200, 404 |

Los dos endpoints de bloqueos existen únicamente para sostener el flujo de creación de reserva; no son funcionalidad de cara al usuario final.

---

## 6. Reglas de negocio

**Vehículos**

- `RN-V01` La placa es única; se normaliza a mayúsculas sin espacios antes de validar.
- `RN-V02` El año debe estar entre 1990 y el año actual + 1.
- `RN-V03` La tarifa diaria debe ser mayor a cero.
- `RN-V04` Un vehículo está disponible en un periodo si no existe ningún `BloqueoDisponibilidad` que se solape con ese periodo.

**Periodos**

- `RN-P01` `FechaFin` debe ser estrictamente posterior a `FechaInicio`.
- `RN-P02` `FechaInicio` no puede ser anterior a hoy (UTC).
- `RN-P03` Solapamiento con extremos abiertos: `[10-15)` y `[15-20)` **no** se solapan.

**Reservas**

- `RN-R01` No se puede reservar un vehículo con un bloqueo solapado → `409 Conflict`.
- `RN-R02` El cliente debe existir antes de crear la reserva.
- `RN-R03` `ValorTotal = TarifaDiaria × días`, donde `días = (FechaFin - FechaInicio).Days`, mínimo 1.
- `RN-R04` La tarifa y la placa se congelan como snapshot al crear la reserva.

**Reportes**

- `RN-RP01` Existe un solo registro por fecha (constraint único).
- `RN-RP02` El reporte del día `D` agrega todas las reservas con `FechaCreacion` en `[D 00:00 UTC, D+1 00:00 UTC)`.
- `RN-RP03` Reprocesar una fecha sobrescribe el registro (operación idempotente).

---

## 7. Comunicación entre VehicleService y BookingService

### 7.1 La decisión de fondo: ¿quién es dueño de la disponibilidad?

El enunciado pide que `VehicleService` responda disponibilidad, pero las reservas viven en `BookingService`. Hay tres caminos:

| Opción | Cómo | Problema |
|---|---|---|
| A. BookingService consulta el catálogo y filtra con sus propias reservas | Un solo dueño de la ocupación | VehicleService **no puede** responder disponibilidad → incumple el requisito |
| B. VehicleService lee la tabla de reservas de BookingService | Trivial de implementar | Rompe el aislamiento de datos; deja de haber microservicios reales |
| **C. VehicleService mantiene sus propios bloqueos de ocupación** ✅ | BookingService le pide reservar el cupo vía HTTP | Requiere coordinación en dos pasos y compensación |

**Se elige C.** VehicleService es dueño del recurso escaso (el vehículo en el tiempo) y por tanto el único autorizado a decidir si un cupo está libre. BookingService es dueño del contrato comercial con el cliente.

### 7.2 Flujo de creación de reserva

```
Cliente ──POST /api/reservas──► BookingService
                                     │
                                1. Valida request (FluentValidation)
                                2. Verifica que el Cliente exista (DB local)
                                     │
                                3. POST /api/vehiculos/{id}/bloqueos ──► VehicleService
                                     │                                        │
                                     │                          transacción SERIALIZABLE:
                                     │                          ¿existe bloqueo solapado?
                                     │                            sí → 409 Conflict
                                     │                            no → INSERT bloqueo
                                     │◄── 201 { bloqueoId, placa, tarifaDiaria } ──┘
                                     │
                                4. Crea la Reserva en DB local (snapshot de placa y tarifa)
                                     │
                                5a. OK  → 201 Created
                                5b. Falla → DELETE /bloqueos/{referenciaExternaId}  (compensación)
                                            → 500 + log de error
```

### 7.3 Detalles de implementación

- **Puerto**: `IVehicleCatalogService` en `Bookings.Application/Abstractions`. Métodos: `ReservarCupoAsync`, `LiberarCupoAsync`. Devuelven `Result<T>`, nunca lanzan.
- **Adapter**: `VehicleCatalogHttpClient` en `Bookings.Infrastructure/Http`, registrado con `IHttpClientFactory` (cliente tipado). Base URL desde configuración.
- **Resiliencia**: timeout de 5 s y reintento simple ante fallos de red/5xx (Polly). Seguro porque el bloqueo es idempotente por `ReferenciaExternaId`.
- **Idempotencia**: el `referenciaExternaId` es el `Guid` de la reserva, generado **antes** de la llamada HTTP.

### 7.4 Trade-off reconocido

Esto es una **saga de dos pasos con compensación**, no una transacción distribuida. Si el proceso de BookingService muere entre el paso 3 y el 5b, queda un bloqueo huérfano.

Mitigación en un sistema productivo: patrón **Outbox** con un broker de mensajes, o bloqueos con expiración. Para esta prueba se implementa la compensación síncrona y se documenta el Outbox como evolución: un broker está fuera del alcance definido y la compensación cubre el caso realista con una fracción de la complejidad.

---

## 8. Diseño del Worker

`RentaFacil.Reporting.Worker` — proyecto Worker Service de .NET, pertenece al bounded context **Booking**.

### Ubicación de la lógica

El Worker no contiene lógica de agregación: es un scheduler que invoca `GenerarReporteDiarioCommand` de `Bookings.Application` vía MediatR. Así la agregación se testea con pruebas unitarias sin levantar el host.

### Acceso a datos

El Worker referencia `Bookings.Application` + `Bookings.Infrastructure` y usa el **mismo `BookingsDbContext`**. No llama a la API por HTTP.

> Justificación: el Worker vive dentro del mismo bounded context que las reservas. Sacarlo por HTTP implicaría exponer un endpoint de volcado masivo y mover miles de registros por red para un cálculo que SQL resuelve mejor. La frontera que importa es la del *contexto*, no la del proceso.

### Ciclo de ejecución

```
ReporteDiarioBackgroundService : BackgroundService
  └── ExecuteAsync
        while (!stoppingToken.IsCancellationRequested)
           ├── calcula la próxima ejecución según CRON  → default: 23:55 diario
           ├── espera
           ├── crea un IServiceScope  (el DbContext es Scoped)
           ├── sender.Send(new GenerarReporteDiarioCommand(fecha))
           └── try/catch: loguea y continúa — un fallo nunca tumba el host
```

Configuración (`appsettings.json`):

```json
"ReportingWorker": {
  "CronExpression": "55 23 * * *",
  "EjecutarAlIniciar": true
}
```

`EjecutarAlIniciar` permite demostrar el Worker sin esperar al horario programado.

### Lógica de `GenerarReporteDiarioCommandHandler`

1. Definir ventana `[fecha 00:00 UTC, fecha+1 00:00 UTC)`.
2. Consultar reservas con proyección agregada en SQL (`GroupBy` traducible, sin traer entidades completas).
3. Calcular: total de reservas, valor total, tipo más reservado, clientes únicos, desglose por tipo.
4. `UPSERT` sobre `ReporteReservasDiarias` por `Fecha`.
5. Loguear resultado estructurado: `{ Fecha, TotalReservas, DuracionMs }`.

### Logs

Serilog a consola + archivo rotativo diario (`logs/worker-.log`).

---

## 9. Migraciones y configuración de SQL Server

### 9.1 docker-compose.yml

```yaml
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: rentafacil-sql
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: "Rent4F4cil!2026"
      MSSQL_PID: Developer
    ports: ["14330:1433"]
    volumes: [ "sqlserver-data:/var/opt/mssql" ]
    healthcheck:
      test: ["CMD-SHELL", "/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P \"$$MSSQL_SA_PASSWORD\" -C -Q 'SELECT 1' || exit 1"]
      interval: 10s
      retries: 10
      start_period: 30s

  vehicle-service:
    build: { context: ., dockerfile: src/VehicleService/RentaFacil.Vehicles.Api/Dockerfile }
    ports: ["5001:8080"]
    depends_on: { sqlserver: { condition: service_healthy } }

  booking-service:
    build: { context: ., dockerfile: src/BookingService/RentaFacil.Bookings.Api/Dockerfile }
    ports: ["5002:8080"]
    environment:
      VehicleService__BaseUrl: "http://vehicle-service:8080"
    depends_on: { sqlserver: { condition: service_healthy } }

  reporting-worker:
    build: { context: ., dockerfile: src/BookingService/RentaFacil.Reporting.Worker/Dockerfile }
    depends_on: { sqlserver: { condition: service_healthy } }

volumes:
  sqlserver-data:
```

### 9.2 Cadenas de conexión

```
Vehicles: Server=sqlserver,1433;Database=RentaFacil_Vehicles;User Id=sa;Password=…;TrustServerCertificate=True;Encrypt=False
Bookings: Server=sqlserver,1433;Database=RentaFacil_Bookings;User Id=sa;Password=…;TrustServerCertificate=True;Encrypt=False
```

En desarrollo local fuera de Docker el host es `localhost,14330` (`appsettings.Development.json`).

> **Sobre el puerto:** el contenedor escucha en `1433` dentro de la red de Docker
> (por eso las cadenas de arriba usan `sqlserver,1433`), pero se publica hacia el
> host en `14330` para no chocar con una instancia de SQL Server instalada
> localmente en la máquina de desarrollo. Desde SSMS se conecta a `localhost,14330`
> con usuario `sa`.

### 9.3 Migraciones

Una migración por cambio lógico, nombre descriptivo:

```bash
# VehicleService
dotnet ef migrations add InicialVehiculos \
  -p src/VehicleService/RentaFacil.Vehicles.Infrastructure \
  -s src/VehicleService/RentaFacil.Vehicles.Api \
  -o Persistence/Migrations

# BookingService
dotnet ef migrations add InicialReservas \
  -p src/BookingService/RentaFacil.Bookings.Infrastructure \
  -s src/BookingService/RentaFacil.Bookings.Api \
  -o Persistence/Migrations
```

**Aplicación**: en `Development` cada API ejecuta `Database.MigrateAsync()` al arrancar, con `EnableRetryOnFailure` porque SQL Server tarda en aceptar conexiones aunque el contenedor ya esté "up". En producción esto sería un job de migración separado; se documenta así en el README.

Configuración de EF Core:
- Fluent API en clases `IEntityTypeConfiguration<T>`, cero Data Annotations.
- `Periodo` y `Dinero` como owned types (`OwnsOne`).
- Enums persistidos como `int` con conversión explícita.
- Queries de lectura con `AsNoTracking()`.
- `decimal(18,2)` explícito para todo monto.
- `DateTime` siempre UTC, columnas `datetime2`.

> **Limitación conocida de EF Core:** no es posible declarar por Fluent API un índice
> que combine una columna de la entidad propietaria con propiedades de un owned type
> mapeado a la misma tabla (dotnet/efcore#11336). El índice
> `IX_Bloqueos_VehiculoId_FechaInicio_FechaFin` se agrega con `migrationBuilder.CreateIndex`
> directamente en la migración. Al regenerar migraciones hay que verificar que ese índice
> siga presente, porque el modelo C# no lo conoce.

---

## 10. Estrategia de pruebas

Solo pruebas unitarias, según el alcance definido.

| Proyecto | Herramientas | Qué cubre |
|---|---|---|
| `Vehicles.Domain.UnitTests` | xUnit, FluentAssertions | `Periodo.SeSolapaCon`, `Placa`, `Dinero`, creación de `Vehiculo` y de `BloqueoDisponibilidad` |
| `Vehicles.Application.UnitTests` | xUnit, Moq, FluentValidation.TestHelper | `RegistrarVehiculoHandler` (éxito, placa duplicada), `ConsultarDisponibilidadHandler` (con y sin bloqueos solapados), `CrearBloqueoHandler` (conflicto), validators |
| `Bookings.Domain.UnitTests` | xUnit, FluentAssertions | Cálculo de `ValorTotal`, invariantes de `Reserva`, `Documento`, `Email` |
| `Bookings.Application.UnitTests` | xUnit, Moq | `CrearReservaHandler`: camino feliz, cliente inexistente, vehículo no disponible (409 del puerto), fallo al persistir → se invoca `LiberarCupoAsync`. `ObtenerHistorialPorClienteHandler` |
| `Reporting.UnitTests` | xUnit, EF InMemory | Agregación correcta, día sin reservas → reporte en ceros, idempotencia (ejecutar dos veces → un solo registro) |

**Casos borde obligatorios**: fechas invertidas, periodo en el pasado, reserva de 1 día, reservas adyacentes (fin = inicio, debe permitirse), tarifa cero o negativa.

**Cobertura**: `coverlet.collector` + `reportgenerator`. El enunciado exige > 10 %; la meta razonable con esta batería es > 60 % en `Domain` y `Application`. El informe HTML se adjunta en `docs/coverage/`.

**Convención de nombres**: `Metodo_Escenario_ResultadoEsperado` — ej. `SeSolapaCon_CuandoFinCoincideConInicio_RetornaFalse`.

---

## 11. Decisiones de diseño y trade-offs

| # | Decisión | Alternativa descartada | Por qué |
|---|---|---|---|
| 1 | **Base de datos por servicio** (misma instancia, bases separadas) | Base compartida | Autonomía real de cada contexto; el costo es que no hay joins ni transacciones cruzadas, que es precisamente el punto |
| 2 | **Disponibilidad en VehicleService** vía tabla de bloqueos | Calcularla en BookingService | Cumple el requisito y pone la decisión de concurrencia donde vive el recurso escaso |
| 3 | **Comunicación HTTP síncrona** | Mensajería con eventos de integración | El flujo de reserva necesita respuesta inmediata (¿hay cupo o no?). Costo: acoplamiento temporal — si VehicleService cae, no se crean reservas (`503`) |
| 4 | **Saga con compensación síncrona** | Outbox + broker | Cubre el caso realista con mucha menos infraestructura. Riesgo del bloqueo huérfano documentado en §7.4 |
| 5 | **MediatR (CQRS)** | Servicios de aplicación clásicos | Separa comandos de consultas, y los `IPipelineBehavior` centralizan validación y logging sin ensuciar cada handler. Costo: indirección |
| 6 | **Patrón `Result<T>`** para errores de negocio | Excepciones de dominio | Vehículo ocupado o cliente inexistente no son casos excepcionales; las excepciones se reservan para fallos reales. Costo: plomería para propagar el `Result` |
| 7 | **Repositorio + `IUnitOfWork`** sobre EF Core | `DbContext` directo en los handlers | Mantiene `Application` libre de EF y permite mockear en pruebas unitarias |
| 8 | **Snapshot de placa y tarifa en la Reserva** | Consultar VehicleService al leer el historial | El historial debe ser inmutable y evita N llamadas HTTP al listar reservas |
| 9 | **Sin FK física entre `Reserva.VehiculoId` y `Vehiculos`** | FK real | Están en bases distintas. La integridad se valida en el flujo de aplicación |
| 10 | **Transacción `Serializable` en el bloqueo** | Bloqueo optimista con reintento | Es la sección crítica del sistema (evitar doble reserva). Volumen bajo: la corrección pesa más que el throughput. Alternativa: índice único filtrado + manejo de `SqlException 2601` |
| 11 | **Worker con acceso directo al `DbContext`** | Worker consumiendo la API por HTTP | Pertenece al mismo bounded context; agregar en SQL es mucho más eficiente que paginar por red |
| 12 | **Serilog + `ProblemDetails`** | `Console.WriteLine` y errores ad-hoc | Logging estructurado y errores consistentes con RFC 7807 |
| 13 | **VOs duplicados por contexto** | Moverlos al `SharedKernel` | Compartir modelos entre bounded contexts es la vía rápida a un monolito distribuido |
| 14 | **Sin autenticación** | JWT + Identity | Fuera del alcance del enunciado. Se documenta dónde entraría |
| 15 | **El dominio recibe `fechaActual` como parámetro** en `Vehiculo.Crear` y `AgregarBloqueo` | Inyectar `IDateTimeProvider` en las entidades | Una entidad de dominio no debe tener dependencias inyectadas: recibe todo lo que necesita para decidir. Mantiene el dominio determinista y los tests sin mocks de reloj. La capa Application resuelve el "ahora" vía `IDateTimeProvider` |

### Patrones de diseño aplicados (mapeo al enunciado)

| Patrón | Dónde |
|---|---|
| **Mediator** | MediatR en toda la capa Application |
| **Command / Query** | `CrearReservaCommand`, `ConsultarDisponibilidadQuery` |
| **Factory** | Métodos estáticos `Vehiculo.Crear()`, `Reserva.Crear()` que devuelven `Result<T>` y garantizan invariantes |
| **Adapter** | `VehicleCatalogHttpClient` adaptando la API REST al puerto `IVehicleCatalogService` |
| **Facade** | Los controladores como fachada delgada sobre los casos de uso |
| **Repository + Unit of Work** | Capa de persistencia |
| **Decorator / Chain of Responsibility** | `IPipelineBehavior` (validación → logging) |

---

## 12. Convención de commits y plan de ejecución

Commits organizados por funcionalidad, formato **Conventional Commits**:

```
feat(vehicles): registrar vehículo con validación de placa única
feat(bookings): crear reserva con bloqueo de cupo en VehicleService
test(bookings): pruebas de solapamiento de periodos
docs: agregar arquitectura al README
```

### Plan por fases (una rama y un commit por fase)

| Fase | Entregable |
|---|---|
| 0 | Estructura de la solución, `SharedKernel`, `docker-compose`, `.editorconfig` |
| 1 | `Vehicles.Domain` + pruebas unitarias de dominio |
| 2 | `Vehicles.Infrastructure` (EF, migración) |
| 3 | `Vehicles.Application` + `Vehicles.Api` (registrar, disponibilidad, bloqueos) + Swagger |
| 4 | `Bookings.Domain` + pruebas unitarias |
| 5 | `Bookings.Infrastructure` (EF, migración, `VehicleCatalogHttpClient`) |
| 6 | `Bookings.Application` + `Bookings.Api` (cliente, reserva, historial) |
| 7 | `Reporting.Worker` + `GenerarReporteDiarioCommand` |
| 8 | Pruebas unitarias de Application y Reporting + informe de cobertura |
| 9 | `README.md` y colección Postman |

---

## Checklist contra el enunciado

- [x] Dos microservicios (`VehicleService`, `BookingService`)
- [x] Worker que procesa las solicitudes del día en tabla de reportes
- [x] SQL Server (contenedor) + EF Core
- [x] API REST con Swagger
- [x] Manejo global de errores y validaciones
- [x] Logs (Serilog: consola + archivo)
- [x] SOLID, DTOs, Clean Architecture + DDD táctico
- [x] Patrones: Mediator, Command, Factory, Facade, Adapter
- [x] Git con commits por funcionalidad
- [x] Pruebas unitarias + cobertura documentada (> 10 %)
- [x] Docker Compose
- [ ] Frontend Angular — fuera de alcance por decisión propia
- [ ] Azure — fuera de alcance por decisión propia

---

*Documento de arquitectura v1.2 — aprobado. Cambio v1.2: puerto del contenedor publicado en 14330.*
