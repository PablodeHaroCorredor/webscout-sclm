---
name: efcore-multitenant-migrations
description: >-
  Use this skill when creating, reviewing, applying, or reverting Entity Framework Core
  migrations for WebScout SCLM. Enforces PostgreSQL/Npgsql standards, Global Query Filters,
  automatic OrganizationId stamping, and synthetic seed datasets.
---

# EF Core Multitenant Migrations

Esta skill define el procedimiento estricto para gestionar el esquema de base de datos relacional de **WebScout SCLM** utilizando Entity Framework Core 9/10 con el proveedor Npgsql (PostgreSQL).

## Principios de Esquema y Migraciones (ADR06, ADR09, ADR10)

1. **Línea de Esquema Única**:
   - Toda la plataforma SCLM comparte un único `WebScoutDbContext` y una única línea secuencial de migraciones EF Core.
   - Prohibido tener contextos paralelos que dupliquen o fragmenten la creación de tablas.
2. **Entorno de Datos Real**:
   - Las migraciones y pruebas de persistencia se ejecutan contra una instancia real de PostgreSQL (vía `docker-compose.dev.yml` o contenedor temporal), **nunca contra EF InMemory**, para garantizar la integridad referencial de claves foráneas, índices únicos, transacciones y tipos nativos (como UUIDs y JSONB).
3. **Filtros Globales de Multitenancy**:
   - Cada migración que añade o modifica tablas de entidades de negocio debe asegurar que la columna `organization_id` (o `OrganizationId`) es de tipo `uuid`, obligatoria (`NOT NULL`) y cuenta con índice de rendimiento para las consultas filtradas.

## Comandos Esenciales de Migración

Ejecutar siempre desde la raíz de la solución referenciando el proyecto de infraestructura (`WebScout.Infrastructure`) como destino de la migración y el proyecto de inicio (`WebScout.Web` o herramienta de migración):

### 1. Añadir una nueva migración
```powershell
dotnet ef migrations add <NombreDescriptivoMigracion> `
  --project src/WebScout.Infrastructure `
  --startup-project src/WebScout.Web `
  --output-dir Persistence/Migrations
```

### 2. Aplicar migraciones pendientes a la base de datos local
```powershell
dotnet ef database update `
  --project src/WebScout.Infrastructure `
  --startup-project src/WebScout.Web
```

### 3. Generar script SQL idempotente para revisión o despliegue
```powershell
dotnet ef migrations script --idempotent `
  --project src/WebScout.Infrastructure `
  --startup-project src/WebScout.Web `
  --output migrations.sql
```

### 4. Revertir la última migración (si no se ha aplicado a producción)
```powershell
dotnet ef migrations remove `
  --project src/WebScout.Infrastructure `
  --startup-project src/WebScout.Web
```

## Convenciones de Modelado Multitenant

Al configurar entidades en el método `OnModelCreating`:
```csharp
// Aplicación del filtro global por tenant
builder.Entity<ScoutActivity>()
    .HasQueryFilter(e => e.OrganizationId == _tenantContext.CurrentTenantId);

// Índice compuesto para consultas frecuentes por fecha dentro de un grupo
builder.Entity<ScoutActivity>()
    .HasIndex(e => new { e.OrganizationId, e.StartDate });
```

Al interceptar `SaveChangesAsync`:
```csharp
foreach (var entry in ChangeTracker.Entries<ITenantScopedEntity>())
{
    if (entry.State == EntityState.Added)
    {
        if (_tenantContext.CurrentTenantId == Guid.Empty)
            throw new InvalidOperationException("No se puede crear un registro multitenant sin un Tenant activo.");
            
        entry.Entity.OrganizationId = _tenantContext.CurrentTenantId;
    }
}
```

## Procedimiento de Seed de Datos Sintéticos

Para el desarrollo local y las pruebas funcionales (PA01-PA19), el seed inicial debe poblar:
1. **Tenant Piloto:** Grupo Scout Lestonnac 330 (slug: `lestonnac`, `OrganizationId` fijo conocido).
2. **Tenant de Contraste:** Grupo Scout San Vicente 412 (slug: `sanvicente`, para validar aislamiento entre grupos).
3. **Estructura Estándar:** Secciones scouts (Castores, Manada, Tropa/Scouts, Escultas, Clan), usuarios sintéticos (Familias A y B, Scouter de sección, Scouter revisor, Coordinador).
4. **Regla de Oro:** **Nunca** introducir datos personales reales de menores, documentos firmados reales ni fotos de familias en el seed.
