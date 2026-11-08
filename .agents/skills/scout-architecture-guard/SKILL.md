---
name: scout-architecture-guard
description: >-
  Use this skill to inspect, validate, and enforce the Clean Modular Monolith
  and Multitenant rules of WebScout SCLM. Ensures Domain isolation,
  OrganizationId tenant stamping, resource-based authorization, and outbox patterns.
---

# Scout Architecture Guard

Esta skill establece y verifica los límites de arquitectura del proyecto **WebScout SCLM**, garantizando que el monolito modular respete estrictamente los patrones de aislamiento multitenant, seguridad y separación de capas.

## Principios Fundamentales de Arquitectura (ADR01 a ADR11)

1. **Separación de Responsabilidades por Capas**:
   - `WebScout.Domain`: Entidades puras, agregados, objetos de valor (`PanoletaConfig`, `PaletteConfig`) e invariantes de negocio. **Prohibido referenciar** EF Core, ASP.NET Core Identity, bibliotecas HTTP o capas superiores.
   - `WebScout.Application`: Casos de uso orquestados, DTOs, puertos/interfaces (`ITenantContext`, `IFileStorage`, `IEmailOutbox`), validaciones FluentValidation y comprobaciones de autorización por recurso.
   - `WebScout.Infrastructure`: Implementaciones técnicas concretas (`WebScoutDbContext`, adaptadores Npgsql, S3/MinIO, Mailpit/SMTP Outbox, hashes criptográficos).
   - `WebScout.Web`: Middleware de resolución de tenant (`TenantResolutionMiddleware`), páginas Razor / MVC, controladores de API, filtros de seguridad y layouts con inyección de tokens CSS.
   - `tools/WebScout.Migration`: Herramientas de seed de tenants y scripts de migración aislados.

2. **Aislamiento Multitenant Estricto (`OrganizationId`)**:
   - Toda entidad de negocio perteneciente a un grupo debe implementar la interfaz `ITenantScopedEntity` (con propiedad `Guid OrganizationId`).
   - El `WebScoutDbContext` debe aplicar **Global Query Filters** automáticamente a todas las entidades multitenant:
     ```csharp
     modelBuilder.Entity<T>().HasQueryFilter(e => e.OrganizationId == _tenantContext.CurrentTenantId);
     ```
   - Al guardar cambios (`SaveChangesAsync`), el contexto debe interceptar la llamada y estampar automáticamente el `OrganizationId` del tenant activo a entidades añadidas o modificadas.
   - Solo procesos federativos auditados de SCLM pueden invocar `.IgnoreQueryFilters()` justificadamente y registrando un `AuditEvent`.

3. **Autorización Basada en Recursos y Capacidades**:
   - No utilizar roles globales planos sin alcance.
   - La comprobación de permisos siempre evalúa: `(Usuario, Organización, Sección, Capacidad, Recurso)`.
   - Se debe verificar la pertenencia en servidor en cada descarga de archivo, exportación CSV o invocación de API.

4. **Inmutabilidad Documental y Almacenamiento Privado**:
   - Los documentos firmados y justificantes de pago subidos son inmutables. Nunca se sobrescribe un PDF existente.
   - El almacenamiento de archivos se realiza en bucket privado (S3/MinIO). Ningún archivo privado se sirve estáticamente desde `wwwroot`.
   - Las descargas se sirven mediante URLs prefirmadas temporales o streaming autenticado a través del controlador web con verificación de permisos.

5. **Outbox Pattern para Efectos Secundarios**:
   - No enviar correos SMTP de forma síncrona dentro de la transacción HTTP/base de datos.
   - Los eventos y correos se persisten en una tabla `OutboxMessages` dentro de la transacción de negocio y un worker en segundo plano se encarga de su despacho con reintentos observables.

## Lista de Verificación en Cada Cambio

Antes de dar por completado un cambio arquitectónico o de código:
- [ ] ¿Algún archivo en `WebScout.Domain` importa librerías de infraestructura o web? (Debe ser NO).
- [ ] ¿Tienen las nuevas entidades `OrganizationId` configurado en el filtro global?
- [ ] ¿Se valida la capacidad y la pertenencia a la organización antes de procesar el comando?
- [ ] ¿Se conservan versiones de documentos modificados en lugar de sobrescribirlos?
- [ ] ¿Se ejecutan los tests de arquitectura (`dotnet test tests/WebScout.ArchitectureTests`)?
