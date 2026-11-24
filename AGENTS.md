# Reglas de Trabajo para Agentes en WebScout SCLM

> Versión 1.0 · Noviembre de 2026.

Este documento establece las instrucciones operativas obligatorias para cualquier agente o desarrollador que trabaje en el repositorio **WebScout SCLM**.

---

## 1. Reglas Innegociables de Seguridad y Protección de Datos

1. **Datos de Menores y Familias:** Prohibido incorporar datos personales reales de menores (nombres, DNIs, números de teléfono, direcciones, alergias, historiales médicos). Todo dato de prueba en seeds o fixtures debe ser 100% sintético.
2. **Secretos y Credenciales:** Nunca commitear cadenas de conexión, contraseñas, secretos de S3 o tokens. Usar `.env` (ignorado por Git) y `dotnet user-secrets`.
3. **Inmutabilidad Documental:** Un PDF firmado o justificante adjunto nunca se sobrescribe. Una corrección añade un nuevo registro versionado con su hash SHA-256 correspondiente.

---

## 2. Límites y Reglas de Arquitectura (Vertical Slice Architecture)

1. **Organización por Vertical Slices (ADR12):**
   - Las funcionalidades se organizan por casos de uso verticales dentro de `Features/<Modulo>/<CasoDeUso>/` (ej: endpoint, request, handler, validator, viewModel).
   - Prohibido el acoplamiento directo entre slices de módulos diferentes; la comunicación se realiza mediante eventos o modelos base de `Common/`.
   - La persistencia, el middleware de subdominio, el almacenamiento S3 y el despacho Outbox residen en `Infrastructure/` como pilares transversales para todos los slices.
2. **Aislamiento Multitenant (`OrganizationId`):**
   - Toda entidad de negocio debe implementar `ITenantScopedEntity`.
   - `WebScoutDbContext` debe tener filtros globales (`HasQueryFilter`) configurados en todas las entidades multitenant.
   - `SaveChangesAsync` estampa automáticamente el `OrganizationId` del tenant en curso.
3. **Autorización:**
   - La comprobación de permisos se realiza siempre en servidor y por recurso, también en descargas de archivos y exportaciones.

---

## 3. Pruebas y Trazabilidad

1. **No usar `EF InMemory`** para validar integridad referencial, concurrencia o aislamiento entre organizaciones. Emplear PostgreSQL real (disponible en `docker-compose.dev.yml`).
2. **Evidencia:** Un HTTP 200 no demuestra aceptación funcional completa. Se debe verificar la regla de negocio y el caso negativo (acceso denegado a otra organización o usuario no autorizado).
3. **Actualización Documental:** Al completar hitos o resolver decisiones, actualizar [Estado del proyecto](docs/estado-proyecto.md).

---

## 4. Ramas de Trabajo

1. **Rama de origen:** Por defecto, toda rama de trabajo (funcionalidades, correcciones, refactorizaciones, documentación, tareas de configuración, etc.) se crea desde `develop`, salvo que se indique expresamente otra rama de origen.
2. **Nomenclatura:** El nombre de la rama sigue la estructura `tipo-de-desarrollo/nombre-descriptivo-de-la-tarea-XX`, donde `XX` es el número del issue que resuelve.
   - Ejemplo: `feature/nuevo-login-9`.
   - Tipos habituales: `feature/`, `fix/`, `refactor/`, `docs/`, `chore/`.
   - El nombre descriptivo va en minúsculas, con palabras separadas por guiones.
3. **Issue previo:** Si la tarea aún no tiene issue, se crea antes de abrir la rama para poder referenciar su número.

---

## 5. Skills Disponibles

Consultar y utilizar las skills en `.agents/skills/`:
- `scout-architecture-guard`: Reglas de arquitectura limpia y multitenancy.
- `efcore-multitenant-migrations`: Migraciones EF Core con Npgsql y seeds sintéticos.
- `scout-theming-wcag`: Fórmulas de contraste WCAG 2.2 AA y renderizado de pañoleta.
- `scout-acceptance-testing`: Ejecución de PA01 a PA19.
- `git-workflow-conventions`: Formato de commits y sincronización con GitHub.
