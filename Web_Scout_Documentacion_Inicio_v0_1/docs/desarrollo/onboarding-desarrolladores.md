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

Comparar la salida con la versión fijada, no con una instalada por casualidad en la máquina. En .NET 10 `dotnet new sln` genera `.slnx` por defecto, según la [documentación del SDK](https://learn.microsoft.com/en-us/dotnet/core/compatibility/sdk/10.0/dotnet-new-sln-slnx-default). Se propone `L330.slnx`; no se añade un fichero vacío para simular una solución.

## Estructura que se creará en E1

```text
L330.slnx
src/L330.Domain/
src/L330.Application/
src/L330.Infrastructure/
src/L330.Web/
tests/L330.Domain.Tests/
tests/L330.IntegrationTests/
tests/L330.WebTests/
docs/
```

Los módulos son carpetas dentro de las capas. Domain no referencia EF, Identity ni Web. Application define casos de uso y puertos; Infrastructure los implementa; Web adapta solicitudes. La primera configuración debe demostrar una dependencia correcta y una prueba útil; no se exige crear proyectos de tests vacíos.

## Datos y servicios locales

Usar una base exclusiva de desarrollo, cuenta de acceso con privilegios mínimos, almacenamiento privado temporal y capturador de correo sin entrega externa. Las conexiones se guardan en secretos de usuario o variables del entorno, nunca en Git. Los nombres exactos de opciones se fijarán con la implementación y se documentarán sin sus valores secretos.

El seed sintético incluirá dos organizaciones para pruebas de aislamiento, secciones, roles, dos familias con respuestas independientes y una cuenta sin sección. Contraseñas de prueba se generan localmente y no se publican en documentación. No usar fotografías, PDFs médicos ni identificadores reales de menores.

## Primer arranque una vez exista código

```powershell
dotnet tool restore
dotnet restore L330.slnx
dotnet build L330.slnx --no-restore
dotnet test L330.slnx --no-build
dotnet run --project src/L330.Web
```

La base debe prepararse con el procedimiento de [migraciones](../architecture/migraciones-ef-core.md), no mediante creación automática alternativa del esquema. Registrar versión de SDK, commit, proveedor y resultado. Si falta un proyecto o configuración, se completa E1 antes de tratar esos comandos como una receta válida.

## Primera contribución

Elegir una tarea preparada del backlog, reproducir el caso con datos sintéticos y demostrar resultado y denegaciones. Añadir solo las pruebas que verifican reglas o riesgos reales del cambio. Actualizar el documento temático y Estado con evidencia, sin convertir recomendaciones en hechos. El objetivo inicial es un recurso privado accesible por su titular y denegado a otra cuenta en todos los canales.
