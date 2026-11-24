---
name: scout-architecture-guard
description: >-
  Use this skill to inspect, validate, and enforce the Vertical Slice Architecture (ADR12)
  and Multitenant rules of WebScout SCLM. Ensures slice cohesion, Infrastructure centralization,
  OrganizationId tenant stamping, resource-based authorization, and outbox patterns.
---

# Scout Architecture Guard

Esta skill establece y verifica los límites de arquitectura del proyecto **WebScout SCLM**, garantizando que el monolito modular respete estrictamente los patrones de Vertical Slice Architecture (VSA), aislamiento multitenant y seguridad.

## Principios Fundamentales de Arquitectura (ADR01 a ADR12)

1. **Organización por Vertical Slices (ADR12)**:
   - `src/WebScout.Web/Features/`: Cada caso de uso vive autocontenido dentro de su módulo (`Features/<Modulo>/<CasoDeUso>/`), agrupando endpoint, comando/consulta, handler, validación y modelo de respuesta.
   - **Prohibido el acoplamiento cruzado rígido entre slices:** Un slice de `Activities` no invoca directamente clases internas de `Orders`. La comunicación se efectúa a través de eventos o modelos compartidos en `Common/`.
   - `src/WebScout.Web/Infrastructure/`: Persistencia relacional (`WebScoutDbContext`), resolución de subdominio (`TenantResolutionMiddleware`), almacenamiento privado S3/MinIO y cola Outbox.
   - `src/WebScout.Web/Common/`: Entidades compartidas, Value Objects (`PanoletaConfig`), políticas de autorización y fórmulas WCAG.
   - `tools/WebScout.Migration`: Herramientas de seed de grupos scout (tenants).

2. **Aislamiento Multitenant Estricto (`OrganizacionId`)**:
   - Toda entidad de negocio perteneciente a un grupo debe implementar la interfaz `IEntidadOrganizacion` (con propiedad `Guid OrganizacionId`).
   - El `WebScoutDbContext` debe aplicar **Global Query Filters** automáticamente a todas las entidades multitenant:
     ```csharp
     modelBuilder.Entity<T>().HasQueryFilter(e => _tenantContext.IsSuperAdmin || e.OrganizacionId == _tenantContext.CurrentTenantId);
     ```
   - Al guardar cambios (`SaveChangesAsync`), el contexto debe interceptar la llamada y estampar automáticamente el `OrganizacionId` del tenant activo a entidades añadidas o modificadas.
   - Solo procesos federativos auditados de SCLM o usuarios con rol `SuperAdmin` pueden operar sin fijar un grupo específico o consultar datos globales.
   - **Validación Cross-Tenant en Peticiones:** Todo usuario autenticado debe pertenecer al `CurrentTenantId` del subdominio en el que navega, bloqueando peticiones con credenciales de otros grupos (`wrong_tenant`).

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
