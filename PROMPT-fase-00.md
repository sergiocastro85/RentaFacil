# Prompt — Fase 0: estructura de la solución

> Pégalo tal cual en Claude CLI, parado en la raíz del repo vacío,
> con `ARCHITECTURE.md` y `CLAUDE.md` ya presentes.

---

Lee `ARCHITECTURE.md` y `CLAUDE.md` completos antes de empezar.

Ejecuta la **Fase 0** del plan de `ARCHITECTURE.md` §12 y nada más.

Alcance exacto de esta fase:

1. Crear `RentaFacil.sln` y **todos** los proyectos vacíos con la estructura de
   carpetas de `ARCHITECTURE.md` §1, usando `dotnet new` (classlib, webapi, worker,
   xunit) y `dotnet sln add`. Los proyectos quedan vacíos salvo lo indicado abajo.

   **IMPORTANTE:** en esta máquina está instalado el SDK de .NET 10, pero el proyecto
   apunta a **`net8.0`**. Pasa `-f net8.0` en **todos** los `dotnet new`. Verifica
   después que ningún `.csproj` haya quedado en `net10.0`.

   En `dotnet new webapi` usa además `--use-controllers` (no minimal APIs) y
   `--no-https` (el TLS local no aporta nada al alcance y complica Docker).

2. Establecer las **referencias entre proyectos** exactamente según la regla de
   dependencias de §1. Verifica explícitamente que `Vehicles.*` no referencie
   `Bookings.*` ni viceversa, y que ningún `Domain` referencie `Infrastructure`.

3. Implementar `RentaFacil.SharedKernel` completo:
   - `Primitives/Entity.cs` — clase base abstracta con `Id` de tipo `Guid` e
     igualdad por identidad.
   - `Primitives/AggregateRoot.cs` — hereda de `Entity`.
   - `Primitives/ValueObject.cs` — igualdad estructural vía
     `GetEqualityComponents()`.
   - `Results/Error.cs` — `Code`, `Description`, `ErrorType`.
   - `Results/ErrorType.cs` — enum: `Validation`, `NotFound`, `Conflict`, `Failure`.
   - `Results/Result.cs` y `Results/Result{T}.cs` — con `Success`, `Failure`,
     conversión implícita desde el valor, y acceso a `Value` que lanza si es fallo.
   - `Abstractions/IUnitOfWork.cs` — `SaveChangesAsync(CancellationToken)`.
   - `Abstractions/IDateTimeProvider.cs` — `DateTime UtcNow { get; }`.

   El `SharedKernel` no referencia ningún paquete NuGet.

4. Crear `Directory.Build.props` en la raíz con: `net8.0`, `LangVersion latest`,
   `Nullable enable`, `ImplicitUsings enable`, `TreatWarningsAsErrors true`.

5. Crear `.editorconfig` con reglas de estilo estándar de C# (indentación de 4
   espacios, `var` cuando el tipo es evidente, `using` fuera del namespace,
   namespaces con file-scoped).

6. Crear `.gitignore` para .NET.

7. Crear `docker-compose.yml` exactamente como aparece en `ARCHITECTURE.md` §9.1.
   Los servicios de las APIs y el Worker quedan declarados aunque los `Dockerfile`
   todavía no existan; crea los tres `Dockerfile` como multi-stage build estándar
   de .NET 8 apuntando a cada proyecto.

7b. Todos los paquetes NuGet de Microsoft (EF Core, extensiones de hosting, etc.)
   deben instalarse en la **versión 8.x**, no en la última disponible, para que sean
   compatibles con `net8.0`.

8. Crear los proyectos de test de `ARCHITECTURE.md` §1 (vacíos, con referencia al
   proyecto que corresponde y a xUnit + FluentAssertions + Moq + coverlet.collector).

**No** implementes entidades, DbContext, controladores, handlers ni migraciones:
eso es de fases posteriores.

Al terminar:
- ejecuta `dotnet build` y `dotnet test` y asegúrate de que ambos pasan;
- levanta `docker compose up -d sqlserver` y confirma que el contenedor queda
  saludable;
- muéstrame el árbol de archivos resultante, un resumen de lo creado y el mensaje
  de commit propuesto. No hagas el commit.
