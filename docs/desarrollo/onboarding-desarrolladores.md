# Incorporación al desarrollo

Versión 0.1 · Propuesta de arranque · 6 de noviembre de 2026.

Este paquete prepara la creación del repositorio. Todavía no incluye solución, proyectos, migraciones ni aplicación ejecutable. Los comandos de compilación siguientes serán aplicables después de crear E1; no se presentan como comprobaciones ya realizadas.

## Preparación

Leer [estado](../estado-proyecto.md), [arquitectura](../architecture/arquitectura-modular.md), [seguridad](../architecture/politica-seguridad-y-permisos.md) y [guía de trabajo](../../AGENTS.md). Obtener un clon autorizado y comprobar rama y cambios antes de editar. No copiar ficheros de entorno, volcados o credenciales del ERP.

Instalar SDK .NET 10 soportado, editor con C#, Git y acceso a PostgreSQL de desarrollo. El equipo fijará una revisión concreta en `global.json`, paquetes centralizados en `Directory.Packages.props` y herramientas locales en `.config/dotnet-tools.json`. La versión del proveedor Npgsql deberá ser compatible con EF Core 10. El uso de contenedores es una opción para reproducibilidad, no un requisito cerrado sin validar el host.

```powershell
dotnet --info
dotnet --list-sdks
```

Comparar la salida con la versión fijada, no con una instalada por casualidad en la máquina. En .NET 10 `dotnet new sln` genera `.slnx` por defecto, según la [documentación del SDK](https://learn.microsoft.com/en-us/dotnet/core/compatibility/sdk/10.0/dotnet-new-sln-slnx-default). La solución se denomina `WebScout.slnx`.

## Estructura de la Solución (Vertical Slice Architecture)

Siguiendo la decisión **ADR12**, el monolito adopta una arquitectura basada en cortes verticales (`Features/`):

```text
WebScout.slnx
src/
  WebScout.Web/
    Features/
      Tenants/
      Branding/
      Activities/
      Inscriptions/
      Orders/
      Gallery/
    Infrastructure/
      Persistence/ (WebScoutDbContext y Global Query Filters)
      Tenancy/ (TenantResolutionMiddleware y ITenantContext)
      Storage/ (Adaptador MinIO/S3)
      Outbox/ (Despacho duradero de avisos)
    Common/ (Modelos compartidos, Value Objects y utilidades WCAG)
tools/
  WebScout.Migration/
tests/
  WebScout.ArchitectureTests/
  WebScout.IntegrationTests/
  WebScout.WebTests/
docs/
```

Cada caso de uso vive autocontenido dentro de su carpeta en `Features/` (endpoint, command/query, handler, validación y viewModel), evitando la dispersión de código en múltiples capas artificiales.

## Datos y servicios locales

Usar los servicios en Docker incluidos en el repositorio (`docker-compose.dev.yml`):
- PostgreSQL 17 (puerto 5432)
- Mailpit (puerto 1025/8025)
- MinIO (puerto 9000/9001)

Las conexiones se guardan en `.env` (excluido de Git) o en secretos de usuario (`dotnet user-secrets`), nunca en archivos versionados.

El seed sintético incluirá dos organizaciones para pruebas de aislamiento (`lestonnac` y `sanvicente`), secciones, roles, dos familias con respuestas independientes y una cuenta sin sección. Contraseñas de prueba se generan localmente y no se publican en documentación. Prohibido usar fotografías, PDFs médicos o identificadores reales de menores.

## Primer arranque una vez exista código

```powershell
dotnet tool restore
dotnet restore WebScout.slnx
dotnet build WebScout.slnx --no-restore
dotnet test WebScout.slnx --no-build
dotnet run --project src/WebScout.Web
```

La base debe prepararse con el procedimiento de [migraciones](../architecture/migraciones-ef-core.md), no mediante creación automática alternativa del esquema. Registrar versión de SDK, commit, proveedor y resultado. Si falta un proyecto o configuración, se completa E1 antes de tratar esos comandos como una receta válida.

## Primera contribución

Elegir una tarea preparada del backlog, reproducir el caso con datos sintéticos y demostrar resultado y denegaciones. Añadir solo las pruebas que verifican reglas o riesgos reales del cambio. Actualizar el documento temático y Estado con evidencia, sin convertir recomendaciones en hechos. El objetivo inicial es un recurso privado accesible por su titular y denegado a otra cuenta en todos los canales.
